using System.ComponentModel.DataAnnotations;
using BonusService.Common;
using BonusService.Postgres;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Pay;

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
[Route("/api/[controller]")]
public sealed class OwnerMaxBonusPayController : ControllerBase
{
    private readonly OwnerByPayRep rep;

    public OwnerMaxBonusPayController(OwnerByPayRep rep)
    {
        this.rep = rep;
    }

    [HttpGet("{id}")]
    public async Task<OwnerMaxBonusPay> GetById([FromRoute][Required]int id, CancellationToken ct)
    {
        return await rep.GetAsync(id, ct);
    }

    [HttpGet("All")]
    public async Task<OwnerMaxBonusPay[]> GetAll(CancellationToken ct)
    {
        return await rep.GetAll().ToArrayAsync(ct);
    }

    [HttpPut]
    public async Task<OwnerMaxBonusPay> Add([Required]OwnerMaxBonusPay entity, CancellationToken ct)
    {
        return await rep.AddAsync(entity, ct);
    }

    [HttpPost]
    public async Task<OwnerMaxBonusPay> Update([Required]OwnerMaxBonusPay entity, CancellationToken ct)
    {
        return await rep.UpdateAsync(entity, ct);
    }

    [HttpDelete("{id}")]
    public async Task Delete([FromRoute][Required]int id, CancellationToken ct)
    {
        await rep.DeleteAsync(id, ct);
    }
}
