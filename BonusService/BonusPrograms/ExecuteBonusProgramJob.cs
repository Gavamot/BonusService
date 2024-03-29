using BonusService.Auth.Policy;
using BonusService.BonusPrograms.ChargedByCapacityBonus;
using BonusService.BonusPrograms.SpendMoneyBonus;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using Hangfire;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BonusService.BonusPrograms;

public sealed class ExecuteBonusProgramJobRequestValidator : AbstractValidator<ExecuteBonusProgramJobRequest>
{
    public ExecuteBonusProgramJobRequestValidator()
    {
        RuleFor(x => x.BonusProgramId).NotEmpty();
        RuleFor(x => x.Now).NotEmpty();
    }
}

public sealed class ExecuteBonusProgramJobCommand: ICommandHandler<ExecuteBonusProgramJobRequest>
{
    private readonly BonusDbContext _bonus;
    private readonly IBackgroundJobClient _jobClient;
    private readonly ILogger<ExecuteBonusProgramJobCommand> _logger;
    public ExecuteBonusProgramJobCommand(BonusDbContext bonus, IBackgroundJobClient jobClient, ILogger<ExecuteBonusProgramJobCommand> logger)
    {
        _bonus = bonus;
        _jobClient = jobClient;
        _logger = logger;
    }

    public async ValueTask<Unit> Handle(ExecuteBonusProgramJobRequest command, CancellationToken ct)
    {
        var bonusProgram = await _bonus.GetBonusProgramById(command.BonusProgramId, ct);
        if (bonusProgram == default) throw new ArgumentException("Бонусная программа не найдена или удалена");

        if (bonusProgram.BonusProgramType == BonusProgramType.SpendMoney)
        {
            _jobClient.Enqueue<SpendMoneyBonusJob>(x => x.ExecuteAsync(null, bonusProgram, command.Now));
        }
        else if (bonusProgram.BonusProgramType == BonusProgramType.ChargedByCapacity)
        {
            _jobClient.Enqueue<ChargedByCapacityBonusJob>(x => x.ExecuteAsync(null, bonusProgram, command.Now));
        }
        else
        {
            throw new NotImplementedException($"BonusProgramId = {bonusProgram.Id} Bonus program type {bonusProgram.BonusProgramType} not implemented yet");
        }
        return await Unit.ValueTask;
    }
}

public record class ExecuteBonusProgramJobRequest(int BonusProgramId, DateTimeOffset Now) :  ICommand;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public sealed class BonusProgramController : ControllerBase
{
    /// <summary>
    /// Запусить job по бонусной программе прямо сейчас 1 раз игнорируя cronExpression в бонусной программе c передачей фейкового текущего времени
    /// Фековое время нужно так как бонусные программы считаются от текущей даты для тестов или если необходимо расчитать за предыдущие периоды поможет этот метод
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.BonusServiceExecute)]
    public async Task ExecuteBonusProgramJob([FromServices] IMediator mediator, [FromBody]ExecuteBonusProgramJobRequest request, CancellationToken ct)
    {
        await mediator.Send(request, ct);
    }
}
