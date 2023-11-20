using BonusService.Bonuses;
using BonusService.Common;
using BonusService.Postgres;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using MongoDB.EntityFrameworkCore;

namespace BonusService.BonuseProgramExecuter;

public sealed class BonusProgramAchievementRequestValidator : AbstractValidator<BonusProgramAchievementRequest>
{
    public BonusProgramAchievementRequestValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
    }
}

public sealed record BonusProgramAchievementResponseItem
{
    public int BonusProgramId { get; set; }
    public string BonusProgramName { get; set; }
    public BonusProgramType Type { get; set; }
    public string LevelName { get; set; }
    public long LevelCondition { get; set; }
    public int LevelAwardPercent  { get; set; }
    public int LevelAwardSum { get; set; }
    public long CurrentSum { get; set; }
    public string? NextLevelName { get; set; }
    public long? NextLevelCondition { get; set; }
    public int? NextLevelAwardPercent  { get; set; }
    public int? NextLevelAwardSum { get; set; }
}
public sealed record BonusProgramAchievementResponse(BonusProgramAchievementResponseItem [] Items);
public sealed record BonusProgramAchievementRequest(Guid PersonId) : IRequest<BonusProgramAchievementResponse>;

public sealed class BonusProgramAchievementCommand : IRequestHandler<BonusProgramAchievementRequest, BonusProgramAchievementResponse>
{
    private readonly MongoDbContext _mongo;
    private readonly PostgresDbContext _postgres;
    private readonly IDateTimeService _dateTimeService;

    public BonusProgramAchievementCommand(
        MongoDbContext mongo,
        IDateTimeService dateTimeService)
    {
        _mongo = mongo;
        _dateTimeService = dateTimeService;
    }

    public async ValueTask<BonusProgramAchievementResponse> Handle(BonusProgramAchievementRequest request, CancellationToken ct)
    {
        var program = new BonusProgramRep().Get();
        var curMonth = _dateTimeService.GetCurrentMonth();
        var payment = _mongo.Sessions.Where(x =>
                x.status == 7
                && x.user != null
                && x.user.clientNodeId == request.PersonId
                && x.user.chargingClientType == 0
                && x.tariff != null
                && x.tariff.BankId == program.BankId
                && x.operation != null
                && x.operation.calculatedPayment > 0
                && x.chargeEndTime>= curMonth.from && x.chargeEndTime< curMonth.to)
            .Sum(x=> x.operation.calculatedPayment ?? 0);
        var levels =  program.ProgramLevels.OrderByDescending(x => x.Level);
        var curLevel = levels.First(x => x.Condition <= payment);
        var nextLevel = levels.SkipWhile(x => x.Condition > payment).Skip(1).FirstOrDefault();

        return new BonusProgramAchievementResponse(new BonusProgramAchievementResponseItem []
        {
            new()
            {
                BonusProgramId = program.Id,
                BonusProgramName = program.Name,
                Type = program.BonusProgramType,
                CurrentSum = payment,

                LevelCondition = curLevel.Condition,
                LevelName = curLevel.Name,
                LevelAwardPercent = curLevel.AwardPercent,
                LevelAwardSum = curLevel.AwardSum,

                NextLevelCondition = nextLevel?.Condition,
                NextLevelName = nextLevel.Name,
                NextLevelAwardPercent = nextLevel.AwardPercent,
                NextLevelAwardSum = nextLevel.AwardSum
            }
        });
    }
}

[ApiController]
[Route("/api/[controller]/[action]")]
public sealed class BonusProgramAchievementController : ControllerBase
{
    [HttpGet]
    public async Task<BonusProgramAchievementResponse> GetPersonAchievement([FromServices] IMediator mediator, [FromQuery]BonusProgramAchievementRequest request, CancellationToken ct)
    {
        var res = await mediator.Send(request, ct);
        return res;
    }
}
