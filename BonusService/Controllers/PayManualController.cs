using BonusService.Bonuses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;
namespace BonusService.Controllers;

public class PayManualDtoValidator : AbstractValidator<PayManualDto>
{
    public PayManualDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.Sum).GreaterThan(0).WithMessage("Сумма должна быть положительной");
        RuleFor(x => x.BankId).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

[Mapper]
public partial class PayManualDtoMapper
{
    public partial BonusManual FromDto(PayManualDto car);
}

public record PayManualDto(Guid PersonId, int BankId, int Sum, string Description, string TransactionId, Guid UserId);

[ApiController]
[Route("/api/[controller]")]
public class PayManualController : ControllerBase
{
    /// <summary>
    /// Списание бонусных баллов оператором
    /// </summary>
    [HttpPost]
    public async Task<BaseResponseEmpty> AccrualManual([FromServices]IBonusService bonusService, [FromBody]PayManualDto transaction)
    {
        var data = new PayManualDtoMapper().FromDto(transaction);
        data = data with { Sum = data.Sum * -1 };
        long res = await bonusService.PayManualAsync(data);
        return new BaseResponse<long>(res);
    }
}
