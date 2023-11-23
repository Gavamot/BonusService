using BonusService.Auth.Policy;
using BonusService.Bonuses;
using BonusService.Pay;
using BonusService.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BonusService.BonuseProgramExecuter;

[ApiController]
[Authorize]
[Route("/[controller]/[action]")]
public sealed class BonusProgramController : ControllerBase//: CrudController<BonusProgram>
{
    private readonly IBonusProgramRep tempRep;
    public BonusProgramController(IBonusProgramRep tempRep)// Change later
    {
        this.tempRep = tempRep;
    }
    [HttpGet]
    [Authorize(Policy = PolicyNames.BonusProgramRead)]
    public BonusProgram[] GetAll(CancellationToken ct) => new [] {tempRep.Get()};
}
