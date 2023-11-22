using System.Text.Json;
using BonusService.Common;
using BonusService.Controllers;
using BonusService.Postgres;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Pay;

public sealed class PayCommonCommand: ICommandHandler<PayTransactionRequest, long>
{
    private readonly PostgresDbContext _postgres;
    private readonly IMediator _mediator;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<PayCommand> _logger;
    public PayCommonCommand(PostgresDbContext postgres, IMediator mediator, IDateTimeService dateTimeService, ILogger<PayCommand> logger)
    {
        _postgres = postgres;
        _mediator = mediator;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }
    public async ValueTask<long> Handle(PayTransactionRequest command, CancellationToken ct)
    {
        var transaction = command.transaction;
        var oldTransaction = await _postgres.Transactions.FirstOrDefaultAsync(x => x.TransactionId == transaction.TransactionId, cancellationToken: ct);
        if (oldTransaction != null) return oldTransaction.BonusSum;
        var bonusBalance = await _mediator.Send(new GetBalanceByBankIdDto(transaction.PersonId, transaction.BankId), ct);
        var bonusSum = Math.Min(bonusBalance, transaction.BonusSum);
        _logger.LogInformation("Запрос на списание {BonusSum}, пользователь имеет {BonusBalance} бонусов",transaction.BonusSum, bonusBalance);
        if (bonusSum <= 0)
        {
            _logger.LogInformation("У пользователя нет средств на списание. Состояние счета не было изменено(не будет сделанно новых записей в бд)");
            return 0;
        }
        transaction.BonusSum = bonusSum * -1;
        transaction.LastUpdated = _dateTimeService.GetNowUtc();
        await _postgres.Transactions.AddAsync(transaction, ct);
        await _postgres.SaveChangesAsync(ct);

        var transactionJson = JsonSerializer.Serialize(transaction);
        _logger.LogInformation("Было списанно со счета пользователя {BonusSum}, транзакция = {Transaction}", transaction.BonusSum, transactionJson);
        return bonusSum;
    }
}
