using System.ComponentModel.DataAnnotations;
using BonusService.Auth.Policy;
using BonusService.BonusPrograms.ChargedByCapacityBonus;
using BonusService.BonusPrograms.SpendMoneyBonus;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
#pragma warning disable CS8618

// ReSharper disable once CheckNamespace
namespace BonusService.BonusPrograms.BonusProgramAchievement;

[Mapper]
public partial class BonusProgramDtoMapper
{
    public partial BonusProgramAchievementDto ToDto(BonusProgram requestDto);
}

[Mapper]
public partial class BonusProgramLevelDtoMapper
{
    public partial BonusProgramAchievementLevelDto ToDto(BonusProgramLevel requestDto);
}

public sealed class BonusProgramAchievementDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public BonusProgramType BonusProgramType { get; set; }
    public string Description { get; set; }
    public DateTimeOffset DateStart { get; set; }
    public DateTimeOffset DateStop { get; set; } = DateTimeOffset.MaxValue;
    public int BankId { get; set; }
    public string ExecutionCron { get; set; }
    public FrequencyTypes FrequencyType { get; set; }
    public int  FrequencyValue { get; set; }
    public List<BonusProgramAchievementLevelDto> ProgramLevels { get; set; }
}

public class BonusProgramAchievementLevelDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public long Condition { get; set; }
    public int AwardPercent  { get; set; }
    public int AwardSum { get; set; }
}

public sealed class BonusProgramAchievementRequestValidator : AbstractValidator<BonusProgramAchievementRequest>
{
    public BonusProgramAchievementRequestValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
    }
}

public sealed record BonusProgramAchievementResponseItem
{
    public BonusProgramAchievementDto BonusProgram { get; set; }
    public long CurrentSum { get; set; }
}


public sealed record BonusProgramAchievementResponse([Required]BonusProgramAchievementResponseItem [] Items);
public sealed record BonusProgramAchievementRequest([Required]string PersonId) : IRequest<BonusProgramAchievementResponse>;

public sealed class BonusProgramAchievementCommand : IRequestHandler<BonusProgramAchievementRequest, BonusProgramAchievementResponse>
{
    private readonly BonusDbContext _bonus;
    private readonly IMediator _mediator;
    private readonly IDateTimeService _dateTimeService;

    public BonusProgramAchievementCommand(
        BonusDbContext bonus,
        IMediator mediator,
        IDateTimeService dateTimeService)
    {
        _bonus = bonus;
        _mediator = mediator;
        _dateTimeService = dateTimeService;
    }

    private ValueTask<long> CalculateAchievementSumAsync(string personId, BonusProgram bonusProgram, Interval interval) =>
        bonusProgram.BonusProgramType switch
        {
            BonusProgramType.SpendMoney => _mediator.Send(new SpendMoneyBonusAchievementRequest(personId, interval, bonusProgram.BankId)),
            BonusProgramType.ChargedByCapacity => _mediator.Send(new ChargedByCapacityAchievementRequest(personId, interval, bonusProgram.BankId)),
            _ => throw new NotImplementedException()
        };

    public async ValueTask<BonusProgramAchievementResponse> Handle(BonusProgramAchievementRequest request, CancellationToken ct)
    {
        var now = _dateTimeService.GetNowUtc();
        var bonusPrograms = await _bonus.GetActiveBonusPrograms(now).ToArrayAsync(ct);
        List<BonusProgramAchievementResponseItem> items = new();
        var mapper = new BonusProgramDtoMapper();

        foreach (var bonusProgram in bonusPrograms)
        {
            try
            {
                var interval = Interval.GetFromNowToFutureDateInterval(bonusProgram.FrequencyType, bonusProgram.FrequencyValue, now);
                if((bonusProgram as IHaveActivePeriod).IsActive(interval) == false) continue;
                var sum = await CalculateAchievementSumAsync(request.PersonId, bonusProgram, interval);
                items.Add(new()
                {
                    BonusProgram = mapper.ToDto(bonusProgram),
                    CurrentSum = sum,
                });
            }
            catch (NotImplementedException)
            {
                // Пока реализованны не все варианты
            }
        }
        return new BonusProgramAchievementResponse(items.ToArray());
    }
}

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public sealed partial class BonusProgramController : ControllerBase
{
    /// <summary>
    /// Получение прогресса достиждений в текущем периоде по бонусной программе
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PolicyNames.PersonRead)]
    public async Task<BonusProgramAchievementResponse> GetPersonAchievement([FromServices] IMediator mediator, [FromQuery][Required]BonusProgramAchievementRequest request, CancellationToken ct)
    {
        var res = await mediator.Send(request, ct);
        return res;
    }
}
