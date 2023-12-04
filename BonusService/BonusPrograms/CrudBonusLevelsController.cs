using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;

// ReSharper disable once CheckNamespace
namespace BonusService.BonusPrograms.BonusProgramLevelsCrud;

public class BonusProgramLevelDto : CrudDto<BonusProgramLevel>
{
    public string? Name { get; set; }
    public int? Level { get; set; }
    public int? BonusProgramId { get; set; }
    public long? Condition { get; set; }
    public int? AwardPercent  { get; set; }
    public int? AwardSum { get; set; }
}

[Mapper(AllowNullPropertyAssignment = false)]
public partial class BonusProgramLevelMapper : IUpdateMapper<BonusProgramLevelDto, BonusProgramLevel>
{
    public partial void Map(BonusProgramLevelDto dto, BonusProgramLevel entity);
}

public class BonusProgramLevelRep : DbEntityRep<BonusProgramLevel>
{
    public BonusProgramLevelRep(PostgresDbContext postgres, IDateTimeService dateTimeService) : base(postgres, dateTimeService)
    {
    }
}

[ApiController]
[Authorize]
[Route("/[controller]/[action]")]
public sealed class BonusLevelsController : CrudController<BonusProgramLevel, BonusProgramLevelDto>
{
    public BonusLevelsController(BonusProgramLevelRep rep) : base(rep, new BonusProgramLevelMapper())
    {

    }
}
