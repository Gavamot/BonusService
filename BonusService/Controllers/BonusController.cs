using BonusService.Bonuses;
using Microsoft.AspNetCore.Mvc;

namespace BonusService.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class BonusController : ControllerBase
{
    private readonly IBonusService _bonusService;
    public BonusController(IBonusService bonusService)
    {
        _bonusService = bonusService;

    }

    /// <summary>
    /// Начисление/Списание бонусных баллов
    /// </summary>
    [HttpPost]
    public async Task ManualAccrual(ManualBonusTransaction transaction)
    {
         await _bonusService.ManualAccrualAsync(transaction);
    }
}
