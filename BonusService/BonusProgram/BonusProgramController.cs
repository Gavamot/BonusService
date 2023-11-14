using BonusService.Bonuses;
using BonusService.Postgres;
using Microsoft.AspNetCore.Mvc;
namespace BonusService.BonuseProgramExecuter;

[ApiController]
[Route("/api/[controller]")]
public sealed class BonusProgramController : ControllerBase
{
    private readonly IBonusProgramRep rep;
    public BonusProgramController(IBonusProgramRep rep)
    {
        this.rep = rep;
    }

    [HttpGet]
    public BonusProgram[] GetAll()
    {
        var p = new [] { rep.Get() };
        return p;
    }
}
