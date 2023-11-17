using System.ComponentModel.DataAnnotations;
using BonusService.Common;
using BonusService.Postgres;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Pay;


public sealed class OwnerByPayValidator : AbstractValidator<OwnerMaxBonusPay>
{
    public OwnerByPayValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.MaxBonusPayPercentages).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
    }
}

public class OwnerByPayRep : DbEntityRep<OwnerMaxBonusPay>
{
    public OwnerByPayRep(PostgresDbContext postgres, IDateTimeService dateTimeService) : base(postgres, dateTimeService)
    {

    }
}

/// <summary>
/// Справочник пользователей
/// </summary>
[ApiController]
[Route("/api/[controller]/[action]")]
public sealed class OwnerMaxBonusPayController : CrudController<OwnerMaxBonusPay>
{
    public OwnerMaxBonusPayController(OwnerByPayRep rep) : base(rep)
    {

    }
}
