using BonusService.Postgres;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Controllers;

public sealed class GetBalanceDtoValidator : AbstractValidator<GetBalanceDto>
{
    public GetBalanceDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.BankId).GreaterThan(0);
    }
}

public sealed record GetBalanceDto(Guid PersonId, int BankId) : IRequest<long>, IBalanceKey;

public sealed class GetPersonBalanceHandler : IRequestHandler<GetBalanceDto, long>
{
    private readonly PostgresDbContext _postgres;
    public GetPersonBalanceHandler(PostgresDbContext postgres)
    {
        _postgres = postgres;
    }

    public async ValueTask<long> Handle(GetBalanceDto request, CancellationToken cancellationToken)
    {
        return await _postgres.Transactions.Where(x => x.PersonId == request.PersonId && x.BankId == request.BankId)
            .SumAsync(x=>x.BonusSum, cancellationToken: cancellationToken);
    }
}

[ApiController]
[Route("/api/[controller]")]
public sealed class BalanceController : ControllerBase
{
    /// <summary>
    /// Получить баланс пользователя
    /// </summary>
    [HttpGet]
    public async Task<long> GetPersonBalance([FromServices]IMediator mediator, [FromQuery]GetBalanceDto data)
    {
        var res = await mediator.Send(data);
        return res;
    }
}
