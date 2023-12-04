using System.ComponentModel.DataAnnotations;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;

// ReSharper disable once CheckNamespace
namespace BonusService.Balance.OwnerByPayCrud;

public sealed class OwnerByPayValidator : AbstractValidator<OwnerMaxBonusPay>
{
    public OwnerByPayValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.MaxBonusPayPercentages).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);
    }
}

public class OwnerByPayDto : CrudDto<OwnerMaxBonusPay>
{
    public int? OwnerId { get; set; }
    public int? MaxBonusPayPercentages { get; set; }
}

[Mapper(AllowNullPropertyAssignment = false)]
public partial class OwnerByPayMapper : IUpdateMapper<OwnerByPayDto, OwnerMaxBonusPay>
{
    public partial void Map(OwnerByPayDto dto, OwnerMaxBonusPay entity);
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
public sealed class OwnerMaxBonusPayController : CrudController<OwnerMaxBonusPay, OwnerByPayDto>
{
    public OwnerMaxBonusPayController(OwnerByPayRep rep) : base(rep, new OwnerByPayMapper())
    {
    }
}
