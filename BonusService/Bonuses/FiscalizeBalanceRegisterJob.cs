using BonusService.Common;
using BonusService.Postgres;
using Microsoft.EntityFrameworkCore;

namespace BonusService.Bonuses;

/// <summary>
/// Подсчет остатнов бонусов на начало прошлого дня для каждого пользователя
/// </summary>
public class FiscalizeBalanceRegisterJob : AbstractJob
{
    private readonly PostgresDbContext _postgres;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<MonthlySumBonusJob> _logger;
    public override string Name => "[Подсчет остатнов счета на начало дня]";

    public FiscalizeBalanceRegisterJob(
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
        var fiscalizedPersons = await _postgres.BalanceRegister
            .GroupBy(x => new {x.PersonId, x.BankId})
            .Select(x => x.OrderByDescending(y => y.Date).First())
            .Select(x=> new { x.PersonId , x.BankId, x.Date, x.Sum })
            .AsNoTracking()
            .ToDictionaryAsync(x=> new BalanceKey(x.PersonId, x.BankId));

        var fiscalDate = _dateTimeService.GetStartOfCurrentDay();
        await _postgres.Transactions
            .GroupBy(x => new BalanceKey(x.PersonId, x.BankId))
            .Select(x => x.Key)
            .Chunk(1000)
            .AsNoTracking()
            .ForEachAsync(async valet =>
            {

                // Переделать на пачки
                try
                {
                    await TryRefiscalizePerson(valet);
                }
                catch (Exception e)
                {
                    logger.LogError(AppEvents.FiscalizeBalanceRegisterJobEvent, e, "Ошибка во время фикализации бонусного счета({valet}) к {to}", valet, fiscalDate.ToString("u"));
                }
            });

        async Task TryRefiscalizePerson(BalanceKey valet)
        {
            DateTimeOffset registerDate = DateTimeOffset.MinValue;
            long registerBonusSum = 0;
            if (fiscalizedPersons.ContainsKey(valet))
            {
                var register = fiscalizedPersons[valet];
                registerDate = register.Date;
                registerBonusSum = register.Sum;
            }
            var fiscalDate = _dateTimeService.GetStartOfCurrentDay();

            var bonusSumFromRegisterToCurrent = await  _postgres.Transactions.Where(x =>
                    x.PersonId == valet.PersonId && x.BankId == valet.BankId &&
                    x.LastUpdated >= registerDate && x.LastUpdated <  fiscalDate)
                .SumAsync(x=> x.BonusSum);

            var fiscalBonusSum = registerBonusSum + bonusSumFromRegisterToCurrent;
            _postgres.BalanceRegister.AddAsync(new BalanceRegister()
            {
                PersonId = valet.PersonId,
                BankId = valet.BankId,
                LastUpdated = _dateTimeService.GetNow(),
                Date = fiscalDate,
                Sum = fiscalBonusSum
            });
            await _postgres.BulkSaveChangesAsync();
        }
    }
}
