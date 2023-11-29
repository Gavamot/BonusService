using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BonusService.Auth.Policy;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
namespace BonusService.Pay;

public sealed class AccrualManualDtoValidator : AbstractValidator<AccrualManualRequestDto>
{
    public AccrualManualDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.BonusSum).NotEmpty().GreaterThan(0).WithMessage("Сумма должна быть положитьельной");
        RuleFor(x => x.BankId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
    }
}

[Mapper]
public sealed partial class AccrualManualDtoMapper
{
    public partial Transaction FromDto(AccrualManualRequestDto requestDto);
}

public sealed record AccrualManualRequestDto([Required]Guid PersonId, [Required]int BankId, [Required]long BonusSum, [Required]string Description, [Required]string TransactionId,  [property: JsonIgnore]string UserName = "") : ICommand;

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

[ApiController]
[Authorize]
[Route("/[controller]")]
public sealed class AccrualManualController : ControllerBase
{
    /// <summary>
    /// Начисление бонусных баллов оператором
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.AccrualManualExecute)]
    public async Task AccrualManual([FromServices]IMediator mediator, [FromBody][Required]AccrualManualRequestDto request, CancellationToken ct)
    {
        request = request with { UserName = HttpContext.GetUserName() };
        await mediator.Send(request,ct);
    }
}
