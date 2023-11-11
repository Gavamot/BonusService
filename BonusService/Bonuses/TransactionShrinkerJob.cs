using System.Data;
using System.Security.Cryptography.X509Certificates;
using BonusService.Common;
using BonusService.Postgres;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;

namespace BonusService.Bonuses;

/// <summary>
/// Так как записи не могут писатся задним чилом можно уплотнить их путем суммы всех значений
/// </summary>
public class TransactionShrinkerJob : AbstractJob
{
    private readonly PostgresDbContext _postgres;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<MonthlySumBonusJob> _logger;
    public override string Name => $"[Shrinker]";

    public TransactionShrinkerJob(
        PostgresDbContext postgres,
        IDateTimeService dateTimeService,
        ILogger<MonthlySumBonusJob> logger) : base(logger)
    {
        _postgres = postgres;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    private record BalanceGroup(Guid PersonId, int BankId, long Sum);
    // https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant
    protected override async Task ExecuteJobAsync()
    {
        var curDay = _dateTimeService.GetStartOfCurrentDay();
        var balance = _postgres.Transactions.Where(x => x.LastUpdated < curDay);

        // В данном алгоритме возвожно консолидация многих транзакций во многие дял каждого счета
        // Но зато работает быстро и решает задачу сжатия. Чем больше chunkSize тем меньше шансов на подобную ситуацию
        int chunkSize = 500_000;
        int cur = 0;
        int i = 1;
        int consolidationLength = 0;

        do
        {
            var transactions = await balance.Skip(cur).Take(chunkSize).AsNoTracking().ToArrayAsync();
            cur += chunkSize;
            var consolidation = transactions.GroupBy(x => new { x.PersonId, x.BankId })
                .Select(x => new { x.Key.PersonId, x.Key.BankId, BonusSum = x.Sum(y => y.BonusSum) }).ToArray();
            var transactionDb = await _postgres.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

            try
            {
                await _postgres.Transactions.BulkInsertAsync(consolidation.Select(x => new Transaction()
                {
                    PersonId = x.PersonId,
                    BankId = x.BankId,
                    BonusSum = x.BonusSum,
                    LastUpdated = curDay,
                    TransactionId = $"shrinker_{x.PersonId:N}_{x.BankId}_{curDay}",
                    Description = $"Консолидация оборотов в разрезу бонусных счетов к {curDay:d.M.yyyy} итерация {i}",
                    Type = TransactionType.Shrink,
                    BonusProgramId = null,
                    BonusBase = null,
                    UserId = null,
                    EzsId = null,
                }));
                await _postgres.BulkDeleteAsync(transactions);
                i++;
                await transactionDb.CommitAsync();
            }
            catch (Exception e)
            {
                logger.
                await transactionDb.RollbackAsync();
            }
        }while(consolidationLength == chunkSize);


    }
}
