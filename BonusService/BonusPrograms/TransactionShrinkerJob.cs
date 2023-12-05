using System.Data;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
namespace BonusService.BonusPrograms;

[Mapper]
public sealed partial class TransactionHistoryMapper
{
    public partial TransactionHistory FromTransaction(Transaction requestDto);
}

/// <summary>
/// Так как записи не могут писатся задним чилом можно уплотнить их путем суммы всех значений
/// </summary>
public class TransactionShrinkerJob : AbstractJob
{
    private readonly PostgresDbContext _postgres;
    private readonly IDateTimeService _dateTimeService;
    public override string Name => $"[Shrinker]";

    public TransactionShrinkerJob(
        PostgresDbContext postgres,
        IDateTimeService dateTimeService,
        ILogger<TransactionShrinkerJob> logger) : base(logger)
    {
        _postgres = postgres;
        _dateTimeService = dateTimeService;
    }


    // https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant
    protected override async Task ExecuteJobAsync(object parameter)
    {
        var curDay = _dateTimeService.GetNowUtc().AddYears(-1);

        // В данном алгоритме возвожно консолидация многих транзакций во многие дял каждого счета
        // Но зато работает быстро и решает задачу сжатия. Чем больше chunkSize тем меньше шансов на подобную ситуацию
        int chunkSize = 100_000;
        int cur = 0;
        int i = 1;
        int consolidationLength = 0;

        logger.LogInformation(AppEvents.TransactionShrinkerEvent, "Начат процесс консолидации транзакций на {curDay} с размером партиции {chunkSize}", curDay, chunkSize);
        do
        {
            var transactions = await _postgres.Transactions.Where(x => x.LastUpdated < curDay)
                .Skip(cur).Take(chunkSize).AsNoTracking().ToArrayAsync();

            consolidationLength = transactions.Length;
            cur += chunkSize;

            var consolidation = transactions.GroupBy(x => new { x.PersonId, x.BankId })
                .Select(x => new { x.Key.PersonId, x.Key.BankId, BonusSum = x.Sum(y => y.BonusSum) }).ToArray();
            await using var transactionDb = await _postgres.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
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
                    BonusProgramId = 0,
                    BonusBase = null,
                    UserName = null,
                    EzsId = null,
                }));
                var mapper = new TransactionHistoryMapper();
                await _postgres.TransactionHistory.BulkInsertAsync(transactions.Where(x => x.Type != TransactionType.Shrink)
                    .Select(x => mapper.FromTransaction(x)));
                await _postgres.BulkDeleteAsync(transactions);
                logger.LogInformation(AppEvents.TransactionShrinkerEvent, "Даннные успешно сконсолидированы({consolidationLength}->{consolidation}) на {curDay} итерация {iteration} с размером партиции {chunkSize}", consolidationLength, consolidation, curDay, i, chunkSize);
                i++;
                await transactionDb.CommitAsync();
            }
            catch (Exception e)
            {
                logger.LogError(AppEvents.TransactionShrinkerEvent, e, "Не удалось сконсолидировать на {curDay} итерация {iteration} с размером партиции {chunkSize}", curDay, i, chunkSize);
                await transactionDb.RollbackAsync();
            }
        }while(consolidationLength == chunkSize);
    }
}
