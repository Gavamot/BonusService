using BonusService.Postgres;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;
namespace BonusService.Pay;

public class PayManualDtoValidator : AbstractValidator<PayManualRequestDto>
{
    public PayManualDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.BonusSum).GreaterThan(0).WithMessage("Сумма должна быть положительной");
        RuleFor(x => x.BankId).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public record PayManualRequestDto(Guid PersonId, int BankId, long BonusSum, string Description, string TransactionId, Guid UserId);

[Mapper]
public partial class PayManualDtoMapper
{
    public partial Transaction FromDto(PayManualRequestDto requestDto);
}

public sealed record PayTransactionRequest(Transaction transaction) : ICommand<long>;

[ApiController]
[Authorize]
[Route("/api/[controller]")]
public sealed class PayManualController : ControllerBase
{
    /// <summary>
    /// Списание бонусных баллов оператором
    /// Возращает число списанных оператором бонусов.
    /// Оператор не может списывать бонусы в минус
    /// </summary>
    [HttpPost]
    public async Task<long> AccrualManual([FromServices]IMediator mediator, [FromBody]PayManualRequestDto request, CancellationToken ct)
    {
        Transaction transaction = new PayManualDtoMapper().FromDto(request);
        transaction.Type = TransactionType.Manual;
        long res = await mediator.Send(new PayTransactionRequest(transaction), ct);
        return res;
    }
}
