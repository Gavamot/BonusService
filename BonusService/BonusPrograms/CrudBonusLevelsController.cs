using System.ComponentModel.DataAnnotations;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.BonusPrograms;

[ApiController]
[Authorize]
[Route("/[controller]/[action]")]
public sealed class BonusLevelsController : CrudController<BonusProgramLevel>
{
    private readonly IDbEntityRep<BonusProgramLevel> _rep;
    public BonusLevelsController(IDbEntityRep<BonusProgramLevel> rep) : base(rep)
    {
        _rep = rep;
    }

    /*[HttpGet("{id:int}")]
    public async Task<BonusProgramLevel?> GetById([FromRoute][Required]int id, CancellationToken ct)
    {
        return await _rep.GetAsync(id, ct);
    }

    [HttpGet]
    public async Task<BonusProgramLevel[]> GetAll(CancellationToken ct)
    {
        return await _rep.GetAll().ToArrayAsync(ct);
    }
    [HttpPost]
    public async Task<BonusProgramLevel> Add([Required]BonusProgramLevel entity, CancellationToken ct)
    {
        return await _rep.AddAsync(entity, ct);
    }

    [HttpPut]
    public async Task<BonusProgramLevel> Update([Required]BonusProgramLevel entity, CancellationToken ct)
    {
        return await _rep.UpdateAsync(entity, ct);
    }

    [HttpDelete("{id:int}")]
    public async Task DeleteById([FromRoute] [Required]int id, CancellationToken ct)
    {
        await _rep.DeleteAsync(id, ct);
    }*/
}
