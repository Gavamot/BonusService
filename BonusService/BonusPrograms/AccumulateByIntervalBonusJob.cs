using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Hangfire.Console;
using MongoDB.Driver;
namespace BonusService.BonusPrograms;

public abstract class AccumulateByIntervalBonusJob : AbstractBonusProgramJob
{
    private readonly MongoDbContext _mongo;
    public AccumulateByIntervalBonusJob(
        MongoDbContext mongo,
        BonusDbContext bonus,
        IDateTimeService dateTimeService,
        ILogger<AccumulateByIntervalBonusJob> logger) : base(logger, bonus, dateTimeService)
    {
        _mongo = mongo;
    }

    private string GenerateTransactionId(int bonusProgram,  string PersonId,int bankId, Interval period)
        => $"{bonusProgram}_{PersonId}_{bankId}_{period.from:yyyy-M-d}-{period.to:yyyy-M-d}";
    public static IQueryable<MongoSession> GetSessionsRequest(MongoDbContext mongo,int bankId, Interval interval) =>
        mongo.Sessions.AsQueryable().Where(x =>
            x.status == MongoSessionStatus.Paid
            && x.user != null
            && x.user.clientNodeId != null
            && x.user.chargingClientType == MongoChargingClientType.IndividualEntity
            && x.tariff != null
            && x.tariff.BankId != null
            && x.tariff.BankId == bankId
            && x.operation != null
            && x.operation.calculatedPayment > 0
            && x.chargeEndTime >= interval.from.UtcDateTime && x.chargeEndTime < interval.to.UtcDateTime);
    protected abstract Func<MongoSession, long> GetConditionField { get; }
    public static (BonusProgramLevel? level, long sum) CalculateBonusSum(long payForCondition ,long totalPay, BonusProgram bonusProgram)
    {
        var level = bonusProgram.ProgramLevels.OrderByDescending(x => x.Level)
            .FirstOrDefault(x=> x.Condition <= payForCondition);
        if (level == null) return (null, 0);
        long bonus = level.AwardSum + (totalPay * level.AwardPercent / 100L);
        return new (level, bonus);
    }
    protected override async Task<BonusProgramJobResult> ExecuteJobAsync(BonusProgram bonusProgram, Interval interval, DateTimeOffset now)
    {
        int bonusProgramId = bonusProgram.Id;
        int bankId = bonusProgram.BankId;
        var bonusProgramMark = GetBonusProgramMark(bonusProgram);

        _logger.LogInformation("{BonusProgramMark} - выборка данных за интервал {Interval} по валюте {BankId}", bonusProgramMark, interval, bankId);
        _ctx.WriteLine($"{bonusProgramMark} - выборка данных за интервал {interval} по валюте {bankId}");
        var data = GetSessionsRequest(_mongo, bankId, interval)
            .GroupBy(x => x.user!.clientNodeId);

        int clientBalanceCount = 0;
        long totalBonusSum = 0;
        var capacity = 4096;
        List<Transaction> transactions = new(4096);

        foreach (var group in data)
        {
            var userName = group.FirstOrDefault()?.user?.clientLogin ?? "null";

            var totalPay = group.Sum(y => y.operation!.calculatedPayment ?? 0);
            var conditionPay = group.Sum(GetConditionField);

            var bonus = CalculateBonusSum(conditionPay, totalPay, bonusProgram);
            if (bonus.sum <= 0)
            {
                _ctx.WriteLine($"У пользователя с PersonId={group.Key} не достаточно достижения для получения бонусов. Он набрал только {totalPay}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(group.Key))
            {
                // Не должно быть токого на всякий случай
                _logger.LogWarning("clientNodeId is empty");
            }

            string clientNodeId = group.Key!;
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
                Description = $"Начислено по {bonusProgramMark}(банк={bankId})login={userName} за {interval}. С суммы платежей {totalPay}.",
                UserName = userName,
                OwnerId = null,
                EzsId = null,
            };
            transactions.Add(transaction);

            _logger.LogInformation("{BonusProgramMark} - Клиент clientNodeId={ClientNodeId} зарядился на {TransactionBonusBase} руб., начислено {TransactionBonusSum} бонусов", bonusProgramMark, clientNodeId, transaction.BonusBase, transaction.BonusSum);
            _ctx.WriteLine($"{bonusProgramMark} - Клиент clientNodeId={clientNodeId} зарядился на {transaction.BonusBase} руб., начислено {transaction.BonusSum} бонусов");
            clientBalanceCount++;
            totalBonusSum += transaction.BonusSum;

            if (transactions.Count < 4096) continue;
            await Bonus.Transactions.BulkInsertAsync(transactions, options =>
            {
                options.InsertIfNotExists = true;
                options.ColumnPrimaryKeyExpression = x => x.TransactionId;
            });
            _logger.LogInformation("{BonusProgramMark} - было начисленно бонусов для {TransactionsCount} пользователей. Даныне в бд добавленны", bonusProgramMark, transactions.Count);
            _ctx.WriteLine($"{bonusProgramMark} - было начисленно бонусов для {transactions.Count} пользователей. Даныне в бд добавленны");
            transactions = new List<Transaction>(capacity);
        }

        if (transactions.Any())
        {
            await Bonus.Transactions.BulkInsertAsync(transactions, options =>
            {
                options.InsertIfNotExists = true;
                options.ColumnPrimaryKeyExpression = x => x.TransactionId;
            });
            _logger.LogInformation("{BonusProgramMark} - было начисленно бонусов для {TransactionsCount} пользователей. Даныне в бд добавленны", bonusProgramMark, transactions.Count);
            _ctx.WriteLine($"{bonusProgramMark} - было начисленно бонусов для {transactions.Count} пользователей. Даныне в бд добавленны");
        }

        if (clientBalanceCount == 0)
        {
            _logger.LogInformation("{BonusProgramMark} по данной бонусной программе данных для начисления бонусов за интервал {Interval} не найдено", bonusProgramMark, interval);
            _ctx.WriteLine($"{bonusProgramMark} по данной бонусной программе данных для начисления бонусов за интервал {interval} не найдено.");
            return new BonusProgramJobResult(interval, 0, 0);
        }

        return new BonusProgramJobResult(interval, clientBalanceCount, totalBonusSum);
    }
}
