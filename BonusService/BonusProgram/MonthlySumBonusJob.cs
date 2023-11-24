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

    public MonthlySumBonusJob(
        IBonusProgramRep rep,
        MongoDbContext mongo,
        PostgresDbContext postgres,
        IDateTimeService dateTimeService,
        ILogger<MonthlySumBonusJob> logger) : base(logger, postgres, dateTimeService)
    {
        _rep = rep;
        _mongo = mongo;
    }

    private string GenerateTransactionId(int bonusProgram,  Guid PersonId,int bankId, DateTimeInterval period)
        => $"{bonusProgram}_{PersonId:N}_{bankId}_{period.from:yyyy-M-d}-{period.to:yyyy-M-d}";

    private (int percentages, long sum) CalculateBonusSum(long totalPay)
    {
        var data =_rep.Get();
        var level = data.ProgramLevels.OrderByDescending(x => x.Level)
            .FirstOrDefault(x=> x.Condition <= totalPay);
        if (level == null) return (0, 0);
        long bonus = totalPay * level.AwardSum / 100L;
        return new (level.AwardSum, bonus);
    }

    protected override async Task<BonusProgramJobResult> ExecuteJobAsync(BonusProgram bonusProgram)
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
                && x.chargeEndTime >= curMonth.from.UtcDateTime && x.chargeEndTime < curMonth.to.UtcDateTime)
            .GroupBy(x => x.user!.clientNodeId);

        var capacity = 4096;
        List<Transaction> transactions = new(4096);

        int clientBalanceCount = 0;
        long totalBonusSum = 0;

        foreach (var group in data)
        {
            var login = group.FirstOrDefault()?.user?.clientLogin ?? "null";
            var totalPay = group.Sum(y => y.operation!.calculatedPayment ?? 0);
            var bonus = CalculateBonusSum(totalPay);
            if (bonus.sum <= 0) { continue; }
            var clientNodeId = group.Key!.Value;

            // Все
            var transaction = new Transaction()
            {
                PersonId = clientNodeId,
                BankId = bankId,
                TransactionId = GenerateTransactionId(bonusProgramId, clientNodeId, bankId, curMonth),
                BonusProgramId = bonusProgramId,
                BonusBase = totalPay,
                BonusSum = bonus.sum,
                Type = TransactionType.Auto,
                LastUpdated = curMonth.from,
                Description = $"Начислено по {bonusProgram.Id}_{bonusProgram.Name}(банк={bankId})login={login} за {curMonth.from.Month} месяц. С суммы платежей {totalPay} к-во процентов {bonus.percentages}.",
                OwnerId = null,
                UserName = null,
                EzsId = null,
            };
            transactions.Add(transaction);

            _logger.LogInformation($"Клиент clientNodeId={clientNodeId} зарядился на {transaction.BonusBase} руб./кВт, начислено {transaction.BonusSum} бонусов");
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
        return new BonusProgramJobResult(clientBalanceCount, totalBonusSum);
    }
}
