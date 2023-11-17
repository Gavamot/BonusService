using BonusService.Common;
using BonusService.Controllers;
using BonusService.Postgres;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Pay;

public sealed class PayCommonCommand: ICommandHandler<PayTransactionRequest, long>
{
    private readonly PostgresDbContext _postgres;
    private readonly IMediator mediator;
    private readonly IDateTimeService dateTimeService;
    public PayCommonCommand(PostgresDbContext postgres, IMediator mediator, IDateTimeService dateTimeService)
    {
        _postgres = postgres;
        this.mediator = mediator;
        this.dateTimeService = dateTimeService;
    }
    public async ValueTask<long> Handle(PayTransactionRequest command, CancellationToken ct)
    {
        var transaction = command.transaction;
        var oldTransaction = await _postgres.Transactions.FirstOrDefaultAsync(x => x.TransactionId == transaction.TransactionId, cancellationToken: ct);
        if (oldTransaction != null) return oldTransaction.BonusSum;
        var bonusBalance = await mediator.Send(new GetBalanceByBankIdDto(transaction.PersonId, transaction.BankId), ct);
        var bonusSum = Math.Min(bonusBalance, transaction.BonusSum);
        if (bonusSum <= 0) return 0;
        transaction.BonusSum = bonusSum * -1;
        transaction.LastUpdated = dateTimeService.GetNowUtc();
        await _postgres.Transactions.AddAsync(transaction, ct);
        await _postgres.SaveChangesAsync(ct);
        return bonusSum;
    }
}
