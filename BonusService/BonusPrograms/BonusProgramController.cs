using BonusService.Auth.Policy;
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

    /// <summary>
    /// Перезапуск всех джоб начисляющих бонусы по бонусным программам
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.BonusServiceWrite)]
    public async Task RestartJobs()
    {
        await _bonusProgramsRunner.RestartAsync();
    }
}
