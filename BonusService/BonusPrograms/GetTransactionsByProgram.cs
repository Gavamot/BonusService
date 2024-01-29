using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BonusService.Auth.Policy;
using BonusService.Balance.BalanceTransactions;
using BonusService.Common;
using BonusService.Common.Postgres;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwaggerExampleAttrLib;
// ReSharper disable once CheckNamespace
namespace BonusService.BonusPrograms.GetTransactionsByProgram;

public sealed class GetTransactionsByProgramRequestValidator : AbstractValidator<GetTransactionsByProgramRequest>
{
    public GetTransactionsByProgramRequestValidator()
    {
        RuleFor(x => x.BonusProgramId).GreaterThan(0).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Limit).GreaterThan(0);
    }
}

public record class GetTransactionsByProgramRequest([Required][property: Example(1)]int BonusProgramId,
    [DefaultValue(1)][property: Example(1)]int Page = 1,
    [DefaultValue(10)][property: Example(10)]int Limit = 10,
    [DefaultValue(null)]DateOnly @DateFrom = default,
    [DefaultValue(null)]DateOnly @DateTo =  default) : IRequest<BalanceTransactionResponse>;


public sealed class ExecuteBonusProgramJobCommand: IRequestHandler<GetTransactionsByProgramRequest, BalanceTransactionResponse>
{
    private readonly BonusDbContext _bonus;
    private readonly ILogger<ExecuteBonusProgramJobCommand> _logger;
    public ExecuteBonusProgramJobCommand(BonusDbContext bonus, ILogger<ExecuteBonusProgramJobCommand> logger)
    {
        _bonus = bonus;
        _logger = logger;
    }

    public async ValueTask<BalanceTransactionResponse> Handle(GetTransactionsByProgramRequest request, CancellationToken ct)
    {
        if (request.DateTo == default)

        {
            request = request with { DateTo =  DateOnly.MaxValue};
        }

        if (request.DateTo < request.DateFrom) throw new ArgumentException("Неверно задан интервал");

        var start = request.DateFrom.ToDateTimeOffset();
        var end = request.DateTo.ToDateTimeOffset();
        var dbQuery = _bonus.Transactions.Where(x =>
            x.BonusProgramId == request.BonusProgramId
            && x.LastUpdated >= start
            && x.LastUpdated < end);

        var count = await dbQuery.CountAsync(ct);
        //var mapper = new BalanceTransactionDtoMapper();

        var transactions = await dbQuery
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .AsNoTracking()
            .ToArrayAsync(ct);

        return new BalanceTransactionResponse(count, transactions);
    }
}

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public sealed partial class BonusProgramController : ControllerBase
{
    /// <summary>
    /// выдает список транзакций по конкретной бонусной программе. https://rnd.sitronics.com/jira/browse/EZSPLAT-394
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PolicyNames.BonusServiceRead)]
    public async Task<BalanceTransactionResponse> GetTransactionsByProgram([FromServices]IMediator mediator,  [FromQuery]GetTransactionsByProgramRequest request)
    {
        return await mediator.Send(request);
    }
}
