using BonusService.Auth.Policy;
using BonusService.BonusPrograms.BonusProgramAchievement;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable once CheckNamespace
namespace BonusService.BonusPrograms.RestartJobs;

[Authorize]
[ApiController]
[Route("/[controller]/[action]")]
public sealed partial class BonusProgramController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.GetBonusProgramAchievementRead)]
    public async Task<BonusProgramAchievementResponse> RestartJobs([FromServices] IMediator mediator, CancellationToken ct)
    {
        throw new NotImplementedException();
        /*var res = await mediator.Send(request, ct);
        return res;*/
    }
}
