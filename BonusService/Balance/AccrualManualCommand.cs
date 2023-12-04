using System.ComponentModel.DataAnnotations;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Balance.PostAccrualManual;

public sealed class AccrualManualCommand : ICommandHandler<AccrualManualRequestDto>
{
    private readonly PostgresDbContext postgres;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<AccrualManualCommand> logger;
    public AccrualManualCommand(PostgresDbContext postgres, IDateTimeService dateTimeService, ILogger<AccrualManualCommand> logger)
    {
        this.postgres = postgres;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }
    public async ValueTask<Unit> Handle([Required]AccrualManualRequestDto command, CancellationToken ct)
    {
        var isTransactionExist = await postgres.Transactions.AnyAsync(x=> x.TransactionId == command.TransactionId, cancellationToken: ct);
        if(isTransactionExist) return Unit.Value;
        var transaction = new AccrualManualDtoMapper().FromDto(command);
        transaction.Type = TransactionType.Manual;
        transaction.LastUpdated = dateTimeService.GetNowUtc();
        await postgres.Transactions.AddAsync(transaction, ct);
        await postgres.SaveChangesAsync(ct);
        logger.LogInformation("Бонусы успешно начисленно пользователю PersonI={PersonId} , BankId={BankId}, сумма {Sum}",
            transaction.PersonId, transaction.BankId, transaction.BonusSum);
        return Unit.Value;
    }
}
