using BonusService.Common;
using BonusService.Postgres;
using Microsoft.EntityFrameworkCore;

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

    // https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant
    protected override async Task ExecuteJobAsync()
    {
        var curDay = _dateTimeService.GetStartOfCurrentDay();
        var balance = _postgres.Transactions.Where(x => x.LastUpdated < curDay)
            .GroupBy(x => new { x.PersonId, x.BankId })
            .Select(x => new { x.Key.PersonId, x.Key.BankId, Sum = x.Sum(y => y.BonusSum) });

        int chunkSize = 100;
        int cur = 0;
        while (true)
        {
            var data = await balance.Skip(cur).Take(chunkSize).ToArrayAsync();

            _postgres.Transactions.AddRange(data.Select(x => new Transaction()
            {
                PersonId = x.PersonId,
                BankId = x.BankId,
                BonusSum = x.Sum,
                LastUpdated = curDay,
                TransactionId = $"shrinker_{x.PersonId:N}_{x.BankId}_{curDay}",
                Description = $"Консолидация оборотов в разрезу бонусных счетов к {curDay:d.M.yyyy}",
                Type = TransactionType.Shrink,
                BonusProgramId = null,
                BonusBase = null,
                UserId = null,
                EzsId = null,
            }));

            _postgres.Transactions.RemoveRange();

            if(data.Length != chunkSize) break;
        }

    }
}
