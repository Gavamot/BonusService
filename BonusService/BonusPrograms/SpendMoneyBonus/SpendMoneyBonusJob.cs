using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Hangfire;
using MongoDB.Driver;
namespace BonusService.BonusPrograms.SpendMoneyBonus;


/// <summary>
/// Начисление бонувов каждый календарный месяц(с 1 по последние число) по уровням от общей суммы затрат персоны
/// https://rnd.sitronics.com/jira/browse/EZSPLAT-244
/// </summary>
public class SpendMoneyBonusJob : AbstractBonusProgramJob
{
    private readonly MongoDbContext _mongo;
    protected override BonusProgramType BonusProgramType => BonusProgramType.SpendMoney;
    public SpendMoneyBonusJob(
        MongoDbContext mongo,
        PostgresDbContext postgres,
        IDateTimeService dateTimeService,
        ILogger<SpendMoneyBonusJob> logger) : base(logger, postgres, dateTimeService)
    {
        _mongo = mongo;
    }

    private string GenerateTransactionId(int bonusProgram,  string PersonId,int bankId, DateTimeInterval period)
        => $"{bonusProgram}_{PersonId:N}_{bankId}_{period.from:yyyy-M-d}-{period.to:yyyy-M-d}";

    private (int percentages, long sum) CalculateBonusSum(long totalPay, BonusProgram bonusProgram)
    {
        var level = bonusProgram.ProgramLevels.OrderByDescending(x => x.Level)
            .FirstOrDefault(x=> x.Condition <= totalPay);
        if (level == null) return (0, 0);
        long bonus = totalPay * level.AwardSum / 100L;
        return new (level.AwardSum, bonus);
    }

    protected override async Task<BonusProgramJobResult> ExecuteJobAsync(BonusProgram bonusProgram, DateTimeOffset now)
    {
        int bonusProgramId = bonusProgram.Id;
        int bankId = bonusProgram.BankId;
        var interval = _dateTimeService.GetDateTimeInterval(bonusProgram.FrequencyType, bonusProgram.FrequencyValue, now);
        var data = _mongo.Sessions.AsQueryable().Where(x =>
                x.status == MongoSessionStatus.Paid
                && x.user != null
                && x.user.clientNodeId != null
                && x.user.chargingClientType == MongoChargingClientType.IndividualEntity
                && x.tariff != null
                && x.tariff.BankId != null
                && x.tariff.BankId == bankId
                && x.operation != null
                && x.operation.calculatedPayment > 0
                && x.chargeEndTime >= interval.from.UtcDateTime && x.chargeEndTime < interval.to.UtcDateTime)
            .GroupBy(x => x.user!.clientNodeId);

        var capacity = 4096;
        List<Transaction> transactions = new(4096);

        int clientBalanceCount = 0;
        long totalBonusSum = 0;

        foreach (var group in data)
        {
            var userName = group.FirstOrDefault()?.user?.clientLogin ?? "null";
            var totalPay = group.Sum(y => y.operation!.calculatedPayment ?? 0);
            var bonus = CalculateBonusSum(totalPay, bonusProgram);
            if (bonus.sum <= 0) { continue; }

            string clientNodeId = group?.Key ?? "null";

            var transaction = new Transaction()
            {
                PersonId = clientNodeId,
                BankId = bankId,
                TransactionId = GenerateTransactionId(bonusProgramId, clientNodeId, bankId, interval),
                BonusProgramId = bonusProgramId,
                BonusBase = totalPay,
                BonusSum = bonus.sum,
                Type = TransactionType.Auto,
                LastUpdated = now,
                Description = $"Начислено по {bonusProgram.Id}_{bonusProgram.Name}(банк={bankId})login={userName} за {interval.from.Month} месяц. С суммы платежей {totalPay} к-во процентов {bonus.percentages}.",
                UserName = userName,
                OwnerId = null,
                EzsId = null,
            };
            transactions.Add(transaction);

            _logger.LogInformation("Клиент clientNodeId={clientNodeId} зарядился на {transaction.BonusBase} руб./кВт, начислено {transaction.BonusSum} бонусов", clientNodeId, transaction.BonusBase, transaction.BonusSum);
            clientBalanceCount++;
            totalBonusSum += transaction.BonusSum;

            if (transactions.Count < 4096) continue;
            await _postgres.Transactions.BulkInsertAsync(transactions, options =>
            {
                options.InsertIfNotExists = true;
                options.ColumnPrimaryKeyExpression = x => x.TransactionId;
            });

            transactions = new List<Transaction>(capacity);
        }

        if (transactions.Any())
        {
            await _postgres.Transactions.BulkInsertAsync(transactions, options =>
            {
                options.InsertIfNotExists = true;
                options.ColumnPrimaryKeyExpression = x => x.TransactionId;
            });
        }
        return new BonusProgramJobResult(interval, clientBalanceCount, totalBonusSum);
    }
}
