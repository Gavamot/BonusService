using System.ComponentModel.DataAnnotations;
using BonusService.Auth.Policy;
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

namespace BonusService.BonusPrograms;

[Mapper]
public partial class BonusProgramDtoMapper
{
    public partial BonusProgramDto ToDto(BonusProgram requestDto);
}

[Mapper]
public partial class BonusProgramLevelDtoMapper
{
    public partial BonusProgramLevelDto ToDto(BonusProgramLevel requestDto);
}

public sealed class BonusProgramDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public BonusProgramType BonusProgramType { get; set; }
    public string Description { get; set; }
    public DateTimeOffset DateStart { get; set; }
    public DateTimeOffset? DateStop { get; set; }
    public int BankId { get; set; }
    public string ExecutionCron { get; set; }
    public FrequencyTypes FrequencyType { get; set; }
    public int  FrequencyValue { get; set; }
    public List<BonusProgramLevelDto> ProgramLevels { get; set; }
}

public class BonusProgramLevelDto
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
    public BonusProgramDto BonusProgram { get; set; }
    public long CurrentSum { get; set; }
}


public sealed record BonusProgramAchievementResponse([Required]BonusProgramAchievementResponseItem [] Items);
public sealed record BonusProgramAchievementRequest([Required]Guid PersonId) : IRequest<BonusProgramAchievementResponse>;

public sealed class BonusProgramAchievementCommand : IRequestHandler<BonusProgramAchievementRequest, BonusProgramAchievementResponse>
{
    private readonly PostgresDbContext _postgres;
    private readonly IMediator _mediator;
    private readonly IDateTimeService _dateTimeService;

    public BonusProgramAchievementCommand(
        PostgresDbContext postgres,
        IMediator mediator,
        IDateTimeService dateTimeService)
    {
        _postgres = postgres;
        _mediator = mediator;
        _dateTimeService = dateTimeService;
    }


    private ValueTask<long> CalculateAchievementSumAsync(Guid personId, BonusProgram bonusProgram)
    {
        var interval = _dateTimeService.GetDateTimeInterval(bonusProgram.FrequencyType, bonusProgram.FrequencyValue);
        switch (bonusProgram.BonusProgramType)
        {
            case BonusProgramType.SpendMoney: return _mediator.Send(new SpendMoneyBonusAchievementRequest(personId, interval, bonusProgram.BankId));
            default: throw new NotImplementedException();
        }
    }

    public async ValueTask<BonusProgramAchievementResponse> Handle(BonusProgramAchievementRequest request, CancellationToken ct)
    {
        var now = _dateTimeService.GetNowUtc();
        var bonusPrograms = await _postgres.GetActiveBonusPrograms(now).ToArrayAsync(ct);
        List<BonusProgramAchievementResponseItem> items = new();
        var mapper = new BonusProgramDtoMapper();

        foreach (var bonusProgram in bonusPrograms)
        {
            var sum = await CalculateAchievementSumAsync(request.PersonId, bonusProgram);
            items.Add(new()
            {
                BonusProgram  = mapper.ToDto(bonusProgram),
                CurrentSum = sum,
            });
        }
        return new BonusProgramAchievementResponse(items.ToArray());
    }
}

[Authorize]
[ApiController]
[Route("/[controller]/[action]")]
public sealed class BonusProgramAchievementController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PolicyNames.GetBonusProgramAchievementRead)]
    public async Task<BonusProgramAchievementResponse> GetPersonAchievement([FromServices] IMediator mediator, [FromQuery][Required]BonusProgramAchievementRequest request, CancellationToken ct)
    {
        var res = await mediator.Send(request, ct);
        return res;
    }
}