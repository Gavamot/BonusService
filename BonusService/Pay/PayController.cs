using BonusService.Auth.Policy;
using BonusService.Postgres;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
namespace BonusService.Pay;

public sealed class PayDtoValidator : AbstractValidator<PayRequestDto>
{
    public PayDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.Payment).GreaterThan(0).WithMessage("Сумма должна быть положительной");
        RuleFor(x => x.BankId).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.EzsId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}

[Mapper]
public sealed partial class PayDtoMapper
{
    public partial Transaction FromDto(PayRequestDto requestDto);
}

public sealed record PayRequestDto(Guid PersonId, int BankId, long Payment, string Description, string TransactionId, Guid EzsId, int OwnerId) : ICommand<long>;

public sealed class PayCommand : ICommandHandler<PayRequestDto, long>
{
    private readonly PostgresDbContext _postgres;
    private readonly IMediator _mediator;
    public PayCommand(PostgresDbContext postgres, IMediator mediator)
    {
        _postgres = postgres;
        _mediator = mediator;
    }

    public async ValueTask<long> Handle(PayRequestDto command, CancellationToken ct)
    {
        Transaction transaction = new PayDtoMapper().FromDto(command);
        transaction.Type = TransactionType.Auto;
        var owner = await _postgres.OwnerMaxBonusPays.FirstOrDefaultAsync(x => x.OwnerId == command.OwnerId, ct);
        var percentages = owner?.MaxBonusPayPercentages ?? 100;
        var bonusSum = (command.Payment * percentages) / 100;
        transaction.BonusSum = bonusSum;
        var res = await _mediator.Send(new PayTransactionRequest(transaction), ct);
        return res;
    }
}

[ApiController]
[Authorize]
[Route("/api/[controller]")]
public sealed class PayController : ControllerBase
{
    /// <summary>
    /// Списание бонусных баллов сервисом оплаты
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.PayExcec)]
    public async Task<long> Pay([FromServices]IMediator mediator, [FromBody]PayRequestDto request, CancellationToken ct)
    {
        long res = await mediator.Send(request, ct);
        return res;
    }
}
