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
namespace BonusService.Balance.PostAccrualManual;

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

public sealed record AccrualManualRequestDto([Required]string PersonId, [Required]int BankId, [Required]long BonusSum, [Required]string Description, [Required]string TransactionId,  [property: JsonIgnore]string UserName = "") : ICommand;

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public sealed partial class BalanceController : ControllerBase
{
    /// <summary>
    /// Начисление бонусных баллов оператором
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.BonusServiceExecute)]
    public async Task AccrualManual([FromServices]IMediator mediator, [FromBody][Required]AccrualManualRequestDto request, CancellationToken ct)
    {
        request = request with { UserName = HttpContext.GetUserName() };
        await mediator.Send(request,ct);
    }
}
