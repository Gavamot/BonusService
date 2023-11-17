using BonusService.Bonuses;
using BonusService.Pay;
using BonusService.Postgres;
using Microsoft.AspNetCore.Mvc;
namespace BonusService.BonuseProgramExecuter;

[ApiController]
[Route("/api/[controller]/[action]")]
public sealed class BonusProgramController : CrudController<BonusProgram>
{
    private readonly IBonusProgramRep tempRep;
    public BonusProgramController(IBonusProgramRep tempRep) : base(null) // Change later
    {
        this.tempRep = tempRep;
    }

    public override async Task<BonusProgram []> GetAll(CancellationToken ct) => new [] {tempRep.Get()};
}
