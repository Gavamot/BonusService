using BonusService.Postgres;
using Microsoft.AspNetCore.Mvc;
namespace BonusService.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class BonusProgramController : ControllerBase
{
    private readonly IBonusProgramRep rep;
    public BonusProgramController(IBonusProgramRep rep)
    {
        this.rep = rep;
    }

    [HttpGet]
    public BaseResponse<BonusProgram[]> Pay()
    {
        var p = new [] { rep.Get() };
        return new BaseResponse<BonusProgram[]>(p);
    }
}
