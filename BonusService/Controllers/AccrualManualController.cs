using System.ComponentModel.DataAnnotations;
using BonusService.Bonuses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;
namespace BonusService.Controllers;

public class AccrualManualDtoValidator : AbstractValidator<AccrualManualDto>
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
public partial class AccrualManualDtoMapper
{
    public partial BonusManual FromDto(AccrualManualDto dto);
}

public record AccrualManualDto([Required]Guid PersonId, [Required]int BankId, [Required]int Sum, string Description, Guid TransactionId, Guid UserId);

[ApiController]
[Route("/api/[controller]")]
public class AccrualManualController : ControllerBase
{
    /// <summary>
    /// Начисление бонусных баллов оператором
    /// </summary>
    [HttpPost]
    public async Task AccrualManual([FromServices]IBonusService bonusService, [FromBody]AccrualManualDto transaction)
    {
        var data = new AccrualManualDtoMapper().FromDto(transaction);
        await bonusService.AccrualManualAsync(data);
    }
}
