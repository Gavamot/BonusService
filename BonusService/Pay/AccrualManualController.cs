using BonusService.Auth.Policy;
using BonusService.Common;
using BonusService.Postgres;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
namespace BonusService.Controllers;

public sealed class AccrualManualDtoValidator : AbstractValidator<AccrualManualRequestDto>
{
    public AccrualManualDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.BonusSum).NotEmpty().GreaterThan(0).WithMessage("Сумма должна быть положитьельной");
        RuleFor(x => x.BankId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

[Mapper]
public sealed partial class AccrualManualDtoMapper
{
    public partial Transaction FromDto(AccrualManualRequestDto requestDto);
}

public sealed record AccrualManualRequestDto(Guid PersonId, int BankId, long BonusSum, string Description, string TransactionId, Guid UserId) : ICommand;

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
    public async ValueTask<Unit> Handle(AccrualManualRequestDto command, CancellationToken ct)
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

[ApiController]
[Authorize]
[Route("/api/[controller]")]
public sealed class AccrualManualController : ControllerBase
{
    /// <summary>
    /// Начисление бонусных баллов оператором
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.AccrualManualExecute)]
    public async Task AccrualManual([FromServices]IMediator mediator, [FromBody]AccrualManualRequestDto request, CancellationToken ct)
    {
        await mediator.Send(request,ct);
    }
}
