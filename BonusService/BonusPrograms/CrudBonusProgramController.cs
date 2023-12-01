using System.ComponentModel.DataAnnotations;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

// ReSharper disable once CheckNamespace
namespace BonusService.BonusPrograms.BonusProgramCrud;
public class BonusProgramDto : CrudCatalogDto
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
public partial class BonusProgramMapper
{
    public partial void Map(BonusProgramDto dto, BonusProgram entity);
}

[ApiController]
[Authorize]
[Route("/[controller]/[action]")]
public sealed partial class BonusProgramController : CrudController<BonusProgram>
{
    private readonly PostgresDbContext _db;
    private readonly IDbEntityRep<BonusProgram> _rep;
    public BonusProgramController(PostgresDbContext db, IDbEntityRep<BonusProgram> rep) : base(rep)
    {
        _db = db;
        _rep = rep;
    }

    [HttpPatch]
    public async Task<IActionResult> Update([Required]BonusProgramDto dto, CancellationToken ct)
    {
        var entity = await _db.BonusPrograms.FirstOrDefaultAsync(x=> x.Id == dto.Id && x.IsDeleted == false, ct);
        if (entity == null) return NotFound();
        new BonusProgramMapper().Map(dto, entity);
        await _rep.UpdateAsync(entity, ct);
        return Ok();
    }

}
