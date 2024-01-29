using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;

// ReSharper disable once CheckNamespace
namespace BonusService.BonusPrograms.BonusProgramCrud;
public class BonusProgramDto : CrudDto<BonusProgram>
{
    public string? Name { get; set; }
    public BonusProgramType? BonusProgramType { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? DateStart { get; set; }
    public DateTimeOffset? DateStop { get; set; }
    public int? BankId { get; set; }
    public string? ExecutionCron { get; set; }
    public FrequencyTypes? FrequencyType { get; set; }
    public int? FrequencyValue { get; set; }
}

[Mapper(AllowNullPropertyAssignment = false)]
public partial class BonusProgramMapper : IUpdateMapper<BonusProgramDto, BonusProgram>
{
    public partial void Map(BonusProgramDto dto, BonusProgram entity);
}

public class BonusProgramRep : DbEntityRep<BonusProgram>
{
    public BonusProgramRep(BonusDbContext bonus, IDateTimeService dateTimeService) : base(bonus, dateTimeService)
    {
    }
}

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public sealed partial class BonusProgramController : CrudController<BonusProgram, BonusProgramDto>
{
    public BonusProgramController(BonusDbContext db, BonusProgramRep rep) : base(rep, new BonusProgramMapper())
    {

    }
}
