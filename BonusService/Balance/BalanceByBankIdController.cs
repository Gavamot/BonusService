using BonusService.Postgres;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Controllers;

public sealed class GetBalanceByBankIdDtoValidator : AbstractValidator<GetBalanceByBankIdDto>
{
    public GetBalanceByBankIdDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.BankId).GreaterThan(0);
    }
}

public sealed record GetBalanceByBankIdDto(Guid PersonId, int BankId) : IRequest<long>, IBalanceKey;

public sealed class GetBalanceByBankIdHandler : IRequestHandler<GetBalanceByBankIdDto, long>
{
    private readonly PostgresDbContext _postgres;
    public GetBalanceByBankIdHandler(PostgresDbContext postgres)
    {
        _postgres = postgres;
    }

    public async ValueTask<long> Handle(GetBalanceByBankIdDto request, CancellationToken ct)
    {
        return await _postgres.Transactions
            .Where(x => x.PersonId == request.PersonId && x.BankId == request.BankId)
            .SumAsync(x=>x.BonusSum, cancellationToken: ct);
    }
}
[ApiController]
[Route("/api/[controller]")]
public sealed class BalanceByBankIdController : ControllerBase
{
    /// <summary>
    /// Получить баланс пользователя по конкретной валюте
    /// </summary>
    [HttpGet]
    public async Task<long> GetBalanceByBankId([FromServices]IMediator mediator, [FromQuery]GetBalanceByBankIdDto data)
    {
        var res = await mediator.Send(data);
        return res;
    }
}
