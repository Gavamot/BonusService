using BonusService.Postgres;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;
namespace BonusService.Controllers;

public sealed class AccrualManualDtoValidator : AbstractValidator<AccrualManualRequestDto>
{
    public AccrualManualDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.Sum).GreaterThan(0).WithMessage("Сумма должна быть положитьельной");
        RuleFor(x => x.BankId).GreaterThan(0);
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

public sealed record AccrualManualRequestDto(Guid PersonId, int BankId, int Sum, string Description, string TransactionId, Guid UserId) : ICommand;

public sealed class AccrualManualCommand : ICommandHandler<AccrualManualRequestDto>
{
    private readonly PostgresDbContext _postgres;
    public AccrualManualCommand(PostgresDbContext postgres)
    {
        _postgres = postgres;
    }
    public async ValueTask<Unit> Handle(AccrualManualRequestDto command, CancellationToken ct)
    {
        var transaction = new AccrualManualDtoMapper().FromDto(command);
        transaction.Type = TransactionType.Manual;
        await _postgres.Transactions.AddAsync(transaction, ct);
        return Unit.Value;
    }
}

[ApiController]
[Route("/api/[controller]")]
public sealed class AccrualManualController : ControllerBase
{
    /// <summary>
    /// Начисление бонусных баллов оператором
    /// </summary>
    [HttpPost]
    public async Task AccrualManual([FromServices]IMediator mediator, [FromBody]AccrualManualRequestDto request, CancellationToken ct)
    {
        var data = new AccrualManualDtoMapper().FromDto(request);
        await mediator.Send(data,ct);
    }
}
