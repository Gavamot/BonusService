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
public sealed class CrudBonusProgramController : ICrudController<BonusProgram>
{
    private readonly IDbEntityRep<BonusProgram> _rep;
    public CrudBonusProgramController(IDbEntityRep<BonusProgram> rep)
    {
        _rep = rep;
    }

    [HttpGet("{id:int}")]
    public async Task<BonusProgram?> GetById([FromRoute][Required]int id, CancellationToken ct)
    {
        return await _rep.GetAsync(id, ct);
    }

    [HttpGet]
    public async Task<BonusProgram[]> GetAll(CancellationToken ct)
    {
        return await _rep.GetAll().ToArrayAsync(ct);
    }

    [HttpPut]
    public async Task<BonusProgram> Add([Required]BonusProgram entity, CancellationToken ct)
    {
        return await _rep.AddAsync(entity, ct);
    }

    [HttpPost]
    public async Task<BonusProgram> Update([Required]BonusProgram entity, CancellationToken ct)
    {
        return await _rep.UpdateAsync(entity, ct);
    }

    [HttpDelete("{id:int}")]
    public async Task DeleteById([FromRoute] [Required]int id, CancellationToken ct)
    {
        await _rep.DeleteAsync(id, ct);
    }
}
