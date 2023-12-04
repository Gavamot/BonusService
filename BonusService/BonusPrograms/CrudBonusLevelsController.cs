using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

// ReSharper disable once CheckNamespace
namespace BonusService.BonusPrograms.BonusProgramLevelsCrud;

public sealed class BonusProgramLevelDtoValidator : AbstractValidator<BonusProgramLevelDto>
{
    public BonusProgramLevelDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class BonusProgramLevelValidator : AbstractValidator<BonusProgramLevel>
{
    public BonusProgramLevelValidator()
    {
        RuleFor(x => x.BonusProgramId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Condition).NotEmpty();
        RuleFor(x => x.Level).NotEmpty();
    }
}


public class BonusProgramLevelDto : CrudDto<BonusProgramLevel>
{
    public int? BonusProgramId { get; set; }
    public string? Name { get; set; }
    public int? Level { get; set; }
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
    private readonly PostgresDbContext _db;
    public BonusLevelsController(BonusProgramLevelRep rep, PostgresDbContext db) : base(rep, new BonusProgramLevelMapper())
    {
        _db = db;
    }

    [HttpGet]
    public async Task<BonusProgramLevel[]> GetAllByBonusProgramId(int bonusProgramId, CancellationToken ct)
    {
       return await _db.BonusProgramsLevels.Where(x => x.BonusProgramId == bonusProgramId).ToArrayAsync(ct);
    }
}
