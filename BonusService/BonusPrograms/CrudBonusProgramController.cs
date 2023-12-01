using System.ComponentModel.DataAnnotations;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
namespace BonusService.BonusPrograms;

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

    /*[HttpGet("{id:int}")]
    public async Task<BonusProgram?> GetById([FromRoute][Required]int id, CancellationToken ct)
    {
        return await _db.BonusPrograms.FirstOrDefaultAsync(x=> x.Id == id && x.IsDeleted == false, ct);
    }

    [HttpGet]
    public async Task<BonusProgram[]> GetAll(CancellationToken ct)
    {
        return await _db.BonusPrograms.Where(x=> x.IsDeleted == false).ToArrayAsync(ct);
    }

    [HttpPost]
    public async Task<BonusProgram> Add([Required]BonusProgram entity, CancellationToken ct)
    {
        var res = await _db.BonusPrograms.AddAsync(entity, ct);
        await _db.SaveChangesAsync(ct);
        return res.Entity;
    }

    [HttpPut]
    public async Task<BonusProgram> Update([Required]BonusProgram entity, CancellationToken ct)
    {
        return await _db.BonusPrograms.Update(entity);
    }

    [HttpDelete("{id:int}")]
    public async Task DeleteById([FromRoute] [Required]int id, CancellationToken ct)
    {
        await _rep.DeleteAsync(id, ct);
    }*/
}
