using BonusService.Bonuses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;
namespace BonusService.Controllers;


public class PayDtoValidator : AbstractValidator<PayDto>
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
public partial class PayDtoMapper
{
    public partial BonusAuto FromDto(PayDto dto);
}

public record PayDto(Guid PersonId, int BankId, int Sum, string Description, Guid TransactionId, int EzsId);


[ApiController]
[Route("/api/[controller]")]
public class PayController : ControllerBase
{
    /// <summary>
    /// Начисление бонусных баллов сервисом оплаты
    /// </summary>
    [HttpPost]
    public async Task Pay([FromServices]IBonusService bonusService, [FromBody]PayDto transaction)
    {
        var data = new PayDtoMapper().FromDto(transaction);
        data = data with { Sum = data.Sum * -1 };
        await bonusService.PayAutoAsync(data);
    }
}
