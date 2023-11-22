using BonusService.Common;
using BonusService.Postgres;
using MongoDB.Driver;

namespace BonusService.Bonuses;


/// <summary>
/// Начисление бонувов каждый календарный месяц(с 1 по последние число) по уровням от общей суммы затрат персоны
/// https://rnd.sitronics.com/jira/browse/EZSPLAT-244
/// </summary>
public class MonthlySumBonusJob : AbstractBonusProgramJob
{
    private readonly IBonusProgramRep _rep;
    private readonly MongoDbContext _mongo;
    private readonly PostgresDbContext _postgres;
    private readonly IDateTimeService _dateTimeService;

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
    }

    private string GenerateTransactionId(int bonusProgram,  Guid PersonId,int bankId, DateTimeInterval period)
        => $"{bonusProgram}_{PersonId:N}_{bankId}_{period.from:yyyy-M-d}";

    private (int percentages, long sum) CalculateBonusSum(long totalPay)
    {
        var data =_rep.Get();
        var level = data.ProgramLevels.OrderByDescending(x => x.Level).FirstOrDefault(x=> x.Condition <= totalPay);
        if (level == null) return (0, 0);
        double percentages = level.AwardSum / 100.0;
        long bonus = (long)(totalPay * percentages);
        return new (level.AwardSum, bonus);
    }

    protected override async Task ExecuteJobAsync(BonusProgram bonusProgram)
    {
        int bonusProgramId = bonusProgram.Id;
        int bankId = bonusProgram.BankId;

        var curMonth = _dateTimeService.GetCurrentMonth();
        var data = _mongo.Sessions.AsQueryable().Where(x => x.status == 7
                && x.user != null
                && x.user.clientNodeId != null
                && x.user.chargingClientType == 0
                && x.tariff != null
                && x.tariff.BankId != null
                && x.tariff.BankId == bankId
                && x.operation != null
                && x.operation.calculatedPayment > 0
                && x.chargeEndTime >= curMonth.from && x.chargeEndTime < curMonth.to)
            .GroupBy(x => x.user!.clientNodeId);

        var capacity = 4096;
        List<Transaction> transactions = new(4096);

        var date = _dateTimeService. GetCurrentMonth().from;

        foreach (var group in data)
        {
            var totalPay = group.Sum(y => y.operation!.calculatedPayment ?? 0);
            var bonus = CalculateBonusSum(totalPay);
            if (bonus.sum <= 0) continue;
            var clientNodeId = group.Key!.Value;
            var transaction = new Transaction()
            {
                PersonId = clientNodeId,
                BankId = bankId,
                TransactionId = GenerateTransactionId(bonusProgramId, clientNodeId, bankId, curMonth),
                BonusProgramId = bonusProgramId,
                BonusBase = totalPay,
                BonusSum = bonus.sum,
                Type = TransactionType.Auto,
                LastUpdated = date,
                UserName = null,
                EzsId = null,
                Description = $"Начислено по {bonusProgram.Name}(банк={bankId}) за {curMonth.from.Month} месяц. С суммы платежей {totalPay}  процентов {bonus.percentages}.",
            };
            transactions.Add(transaction);
            if (transactions.Count >= 4096)
            {
                await _postgres.Transactions.BulkInsertAsync(transactions, options =>
                {
                    options.InsertIfNotExists = true;
                    options.ColumnPrimaryKeyExpression = x => new { x.PersonId, x.BankId, x.LastUpdated, x.BonusProgramId };
                });
                transactions = new List<Transaction>(capacity);
            }
        }

    }
}
