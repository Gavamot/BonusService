using BonusService.Common;
using BonusService.Postgres;
using Microsoft.EntityFrameworkCore;

namespace BonusService.Bonuses;


/// <summary>
/// Начисление бонувов каждый календарный месяц(с 1 по последние число) по уровням от общей суммы затрат персоны
/// https://rnd.sitronics.com/jira/browse/EZSPLAT-244
/// </summary>
public class MonthlySumBonusJob : AbstractJob
{
    private readonly IBonusProgramRep _rep;
    private readonly MongoDbContext _mongo;
    private readonly PostgresDbContext _postgres;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<MonthlySumBonusJob> _logger;

    public readonly ProgramTypes programType = ProgramTypes.PeriodicalMonthlySumByLevels;
    public override string Name => "[Кешбэк]";

    public MonthlySumBonusJob(
        IBonusProgramRep rep,
        MongoDbContext mongo,
        PostgresDbContext postgres,
        IDateTimeService dateTimeService,
        ILogger<MonthlySumBonusJob> logger) : base(logger)
    {
        _rep = rep;
        _mongo = mongo;
        _postgres = postgres;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    private string GenerateTransactionId(int bankId, Guid PersonId, DateTimeInterval period)
        => $"cashback_{PersonId:N}_{bankId}_{period.from:yyyy-M-d}";

    private (int percentages, long sum) CalculateBonusSum(long totalPay)
    {
        var data =_rep.Get();
        var level = data.ProgramLevels.OrderByDescending(x => x.Level).FirstOrDefault(x=> x.Condition <= totalPay);
        if (level == null) return (0, 0);
        double percentages = level.Benefit / 100.0;
        long bonus =(long)(totalPay * percentages);
        return new (level.Benefit, bonus);
    }

    protected override async Task ExecuteJobAsync()
    {
        var curMonth = _dateTimeService.GetCurrentMonth();
        var data = _mongo.Sessions.Where(x => x.status == 7
                && x.user != null
                && x.user.clientNodeId != null
                && x.tariff != null
                && x.tariff.BankId != null
                && x.operation != null
                && x.operation.calculatedPayment > 0
                && x.user.chargingClientType == 0
                && x.chargeEndTime >= curMonth.from && x.chargeEndTime < curMonth.to)
            .GroupBy(x => new { x.tariff!.BankId, x.user!.clientNodeId})
            .AsNoTracking();

        List<Transaction> transactions = new(4096);
        var bonusProgramId = _rep.Get().Id;
        foreach (var x in data)
        {
            var totalPay = x.Sum(y => y.operation!.calculatedPayment ?? 0);
            var bonus = CalculateBonusSum(totalPay);
            if (bonus.sum <= 0) continue;
            var transaction = new Transaction()
            {
                PersonId = x.Key.clientNodeId!.Value,
                BankId = x.Key.BankId!.Value,
                TransactionId = GenerateTransactionId(x.Key.BankId!.Value, x.Key.clientNodeId!.Value, curMonth),
                BonusProgramId = bonusProgramId,
                BonusBase = totalPay,
                BonusSum = bonus.sum,
                Type = TransactionType.Auto,
                LastUpdated = _dateTimeService.GetNow(),
                UserId = null,
                EzsId = null,
                Description = $"Начислено по {Name} за {curMonth.from.Month} месяц. С суммы платежей {totalPay}  процентов {bonus.percentages}.",
            };
            transactions.Add(transaction);
        }
        await _postgres.Transactions.BulkInsertAsync(transactions);
    }
}
