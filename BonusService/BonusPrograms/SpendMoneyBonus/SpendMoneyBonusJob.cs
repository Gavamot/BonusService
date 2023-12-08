using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Hangfire.Console;
using Microsoft.EntityFrameworkCore;
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

    private string GenerateTransactionId(int bonusProgram,  string PersonId,int bankId, DateInterval period)
        => $"{bonusProgram}_{PersonId:N}_{bankId}_{period.from:yyyy-M-d}-{period.to:yyyy-M-d}";

    private (int percentages, long sum) CalculateBonusSum(long totalPay, BonusProgram bonusProgram)
    {
        var level = bonusProgram.ProgramLevels.OrderByDescending(x => x.Level)
            .FirstOrDefault(x=> x.Condition <= totalPay);
        if (level == null) return (0, 0);
        long bonus = totalPay * level.AwardSum / 100L;
        return new (level.AwardSum, bonus);
    }


    protected override async Task<BonusProgramJobResult> ExecuteJobAsync(BonusProgram bonusProgram, DateInterval interval, DateTimeOffset now)
    {
        int bonusProgramId = bonusProgram.Id;
        int bankId = bonusProgram.BankId;
        var bonusProgramMark = GetBonusProgramMark(bonusProgram);

        _logger.LogInformation("{BonusProgramMark} - выборка данных за интервал {Interval} по валюте {BankId}", bonusProgramMark, interval, bankId);
        _ctx.WriteLine($"{bonusProgramMark} - выборка данных за интервал {interval} по валюте {bankId}");
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

        int clientBalanceCount = 0;
        long totalBonusSum = 0;
        var capacity = 4096;
        List<Transaction> transactions = new(4096);

        foreach (var group in data)
        {
            var userName = group.FirstOrDefault()?.user?.clientLogin ?? "null";
            var totalPay = group.Sum(y => y.operation!.calculatedPayment ?? 0);
            var bonus = CalculateBonusSum(totalPay, bonusProgram);
            if (bonus.sum <= 0)
            {
                _ctx.WriteLine($"У пользователя с PersonId={group.Key} не достаточно достижения для получения бонусов. Он набрал только {totalPay}");
                continue;
            }

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
                Description = $"Начислено по {bonusProgramMark}(банк={bankId})login={userName} за {interval}. С суммы платежей {totalPay} к-во процентов {bonus.percentages}.",
                UserName = userName,
                OwnerId = null,
                EzsId = null,
            };
            transactions.Add(transaction);

            _logger.LogInformation("{BonusProgramMark} - Клиент clientNodeId={clientNodeId} зарядился на {transactionBonusBase} руб./кВт, начислено {transactionBonusSum} бонусов", bonusProgramMark, clientNodeId, transaction.BonusBase, transaction.BonusSum);
            _ctx.WriteLine($"{bonusProgramMark} - Клиент clientNodeId={clientNodeId} зарядился на {transaction.BonusBase} руб./кВт, начислено {transaction.BonusSum} бонусов");
            clientBalanceCount++;
            totalBonusSum += transaction.BonusSum;

            if (transactions.Count < 4096) continue;
            await _postgres.Transactions.BulkInsertAsync(transactions, options =>
            {
                options.InsertIfNotExists = true;
                options.ColumnPrimaryKeyExpression = x => x.TransactionId;
            });
            _logger.LogInformation("{BonusProgramMark} - было начисленно бонусов для {transactionsCount} пользователей. Даныне в бд добавленны", bonusProgramMark, transactions.Count);
            _ctx.WriteLine($"{bonusProgramMark} - было начисленно бонусов для {transactions.Count} пользователей. Даныне в бд добавленны");
            transactions = new List<Transaction>(capacity);
        }

        if (transactions.Any())
        {
            await _postgres.Transactions.BulkInsertAsync(transactions, options =>
            {
                options.InsertIfNotExists = true;
                options.ColumnPrimaryKeyExpression = x => x.TransactionId;
            });
            _logger.LogInformation("{BonusProgramMark} - было начисленно бонусов для {transactionsCount} пользователей. Даныне в бд добавленны", bonusProgramMark, transactions.Count);
            _ctx.WriteLine($"{bonusProgramMark} - было начисленно бонусов для {transactions.Count} пользователей. Даныне в бд добавленны");
        }

        if (clientBalanceCount == 0)
        {
            _logger.LogInformation("{BonusProgramMark} по данной бонусной программе данных для начисления бонусов за интервал {Interval} не найдено");
            _ctx.WriteLine($"{bonusProgramMark} по данной бонусной программе данных для начисления бонусов за интервал {interval} не найдено.");
            return new BonusProgramJobResult(interval, 0, 0);
        }

        return new BonusProgramJobResult(interval, clientBalanceCount, totalBonusSum);
    }
}
