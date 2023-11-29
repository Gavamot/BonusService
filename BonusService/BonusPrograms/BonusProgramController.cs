using BonusService.Auth.Policy;
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
public sealed class BonusProgramController : ControllerBase//: CrudController<BonusProgram>
{
    private readonly PostgresDbContext _postgres;
    private readonly IDateTimeService _dateTimeService;
    public BonusProgramController(PostgresDbContext postgres, IDateTimeService dateTimeService)// Change later
    {
        _postgres = postgres;
        _dateTimeService = dateTimeService;
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.BonusProgramRead)]
    public async Task<BonusProgram[]> GetAll(CancellationToken ct) =>
        await _postgres.GetActiveBonusPrograms(_dateTimeService.GetNowUtc()).ToArrayAsync(ct);
}
