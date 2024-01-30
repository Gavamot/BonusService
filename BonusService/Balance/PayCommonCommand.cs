using System.Text.Json;
using BonusService.Balance.GetBalance;
using BonusService.Balance.Pay;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BonusService.Balance;
public sealed record PayTransactionRequest(Transaction transaction) : ICommand<long>;
public sealed class PayCommonCommand: ICommandHandler<PayTransactionRequest, long>
{
    private readonly BonusDbContext _bonus;
    private readonly IMediator _mediator;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<PayCommonCommand> _logger;
    public PayCommonCommand(BonusDbContext bonus, IMediator mediator, IDateTimeService dateTimeService, ILogger<PayCommonCommand> logger)
    {
        _bonus = bonus;
        _mediator = mediator;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }
    public async ValueTask<long> Handle(PayTransactionRequest command, CancellationToken ct)
    {
        var transaction = command.transaction;
        var oldTransaction = await _bonus.Transactions.FirstOrDefaultAsync(x => x.TransactionId == transaction.TransactionId, cancellationToken: ct);
        if (oldTransaction != null) return oldTransaction.BonusSum;
        var request = new GetBalanceByBankIdRequest(transaction.PersonId, transaction.BankId);
        var bonusBalance = await _mediator.Send(request, ct);
        var bonusSum = Math.Min(bonusBalance, transaction.BonusSum);
        _logger.LogInformation("Запрос на списание {BonusSum}, пользователь имеет {BonusBalance} бонусов",transaction.BonusSum, bonusBalance);
        if (bonusSum <= 0)
        {
            _logger.LogInformation("У пользователя нет средств на списание. Состояние счета не было изменено(не будет сделанно новых записей в бд)");
            return 0;
        }
        transaction.BonusSum = bonusSum * -1;
        transaction.LastUpdated = _dateTimeService.GetNowUtc();
        await _bonus.Transactions.AddAsync(transaction, ct);
        await _bonus.SaveChangesAsync(ct);

        var transactionJson = JsonSerializer.Serialize(transaction);
        _logger.LogInformation("Было списанно со счета пользователя {BonusSum}, транзакция = {Transaction}", transaction.BonusSum, transactionJson);
        return bonusSum;
    }
}
