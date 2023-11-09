using BonusService.Bonuses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;
namespace BonusService.Controllers;

public sealed class PayDtoValidator : AbstractValidator<PayDto>
{
    public PayDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.Sum).GreaterThan(0).WithMessage("Сумма должна быть положительной");
        RuleFor(x => x.BankId).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.EzsId).NotEmpty();
    }
}

[Mapper]
public sealed partial class PayDtoMapper
{
    public partial BonusAuto FromDto(PayDto dto);
}

public sealed record PayDto(Guid PersonId, int BankId, int Sum, string Description, string TransactionId, Guid EzsId);


[ApiController]
[Route("/api/[controller]")]
public sealed class PayController : ControllerBase
{
    /// <summary>
    /// Списание бонусных баллов сервисом оплаты
    /// </summary>
    [HttpPost]
    public async Task<long> Pay([FromServices]IBonusService bonusService, [FromBody]PayDto transaction)
    {
        var data = new PayDtoMapper().FromDto(transaction);
        data = data with { Sum = data.Sum * -1 };
        long res = await bonusService.PayAutoAsync(data);
        return res;
    }
}
