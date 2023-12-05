using BonusService.Auth.Policy;
using BonusService.BonusPrograms.BonusProgramAchievement;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable once CheckNamespace
namespace BonusService.BonusPrograms.RestartJobs;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public sealed partial class BonusProgramController : ControllerBase
{
    private readonly IBonusProgramsRunner _bonusProgramsRunner;

    public BonusProgramController(IBonusProgramsRunner bonusProgramsRunner)
    {
        _bonusProgramsRunner = bonusProgramsRunner;
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.GetBonusProgramAchievementRead)]
    public async Task RestartJobs()
    {
        await _bonusProgramsRunner.RestartAsync();
    }
}
