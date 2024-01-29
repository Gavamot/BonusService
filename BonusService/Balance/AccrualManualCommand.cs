using System.ComponentModel.DataAnnotations;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Mediator;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Balance.PostAccrualManual;

public sealed class AccrualManualCommand : ICommandHandler<AccrualManualRequestDto>
{
    private readonly BonusDbContext _bonus;
    private readonly IDateTimeService dateTimeService;
    private readonly ILogger<AccrualManualCommand> logger;
    public AccrualManualCommand(BonusDbContext bonus, IDateTimeService dateTimeService, ILogger<AccrualManualCommand> logger)
    {
        this._bonus = bonus;
        this.dateTimeService = dateTimeService;
        this.logger = logger;
    }
    public async ValueTask<Unit> Handle([Required]AccrualManualRequestDto command, CancellationToken ct)
    {
        var isTransactionExist = await _bonus.Transactions.AnyAsync(x=> x.TransactionId == command.TransactionId, cancellationToken: ct);
        if(isTransactionExist) return Unit.Value;
        var transaction = new AccrualManualDtoMapper().FromDto(command);
        transaction.Type = TransactionType.Manual;
        transaction.LastUpdated = dateTimeService.GetNowUtc();
        await _bonus.Transactions.AddAsync(transaction, ct);
        await _bonus.SaveChangesAsync(ct);
        logger.LogInformation("Бонусы успешно начисленно пользователю PersonI={PersonId} , BankId={BankId}, сумма {Sum}",
            transaction.PersonId, transaction.BankId, transaction.BonusSum);
        return Unit.Value;
    }
}
