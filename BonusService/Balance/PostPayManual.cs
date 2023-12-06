using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BonusService.Auth.Policy;
using BonusService.Common;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;

// ReSharper disable once CheckNamespace
namespace BonusService.Balance.PayManual;

public class PayManualDtoValidator : AbstractValidator<PayManualRequestDto>
{
    public PayManualDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.BonusSum).NotEmpty().GreaterThan(0).WithMessage("Сумма должна быть положительной");
        RuleFor(x => x.BankId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
    }
}

public record PayManualRequestDto([Required]string PersonId, [Required]int BankId, [Required]long BonusSum, [Required]string Description, [Required]string TransactionId, [property: JsonIgnore]string UserName = "");

[Mapper]
public partial class PayManualDtoMapper
{
    public partial Transaction FromDto(PayManualRequestDto requestDto);
}

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public sealed partial class BalanceController : ControllerBase
{
    /// <summary>
    /// Списание бонусных баллов оператором
    /// Возращает число списанных оператором бонусов.
    /// Оператор не может списывать бонусы в минус
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.AccrualManualExecute)]
    public async Task<long> PayManual([FromServices]IMediator mediator, [FromBody][Required]PayManualRequestDto request, CancellationToken ct)
    {
        request = request with { UserName = HttpContext.GetUserName() };
        Transaction transaction = new PayManualDtoMapper().FromDto(request);
        transaction.Type = TransactionType.Manual;
        long res = await mediator.Send(new PayTransactionRequest(transaction), ct);
        return res;
    }
}
