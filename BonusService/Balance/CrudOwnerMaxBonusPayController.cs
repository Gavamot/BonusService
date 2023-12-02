using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BonusService.Balance;

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
[Authorize]
[Route("/[controller]/[action]")]
public sealed class OwnerMaxBonusPayController : CrudController<OwnerMaxBonusPay>
{
    public OwnerMaxBonusPayController(OwnerByPayRep rep) : base(rep)
    {
    }
}