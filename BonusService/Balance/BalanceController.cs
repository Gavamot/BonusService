using BonusService.Auth.Policy;
using BonusService.Postgres;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Controllers;

public sealed class GetPersonBalanceRequestDtoValidator : AbstractValidator<GetPersonBalanceRequestDto>
{
    public GetPersonBalanceRequestDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
    }
}

public record GetPersonBalanceRequestDto(Guid PersonId) : IRequest<GetPersonBalanceResponseDto>;
public record GetPersonBalanceResponseDto(BalanceResponseItemDto[] items);
public record BalanceResponseItemDto(int BankId, long Sum);

public sealed class GetPersonBalanceCommand : IRequestHandler<GetPersonBalanceRequestDto, GetPersonBalanceResponseDto>
{
    private readonly PostgresDbContext _postgres;
    public GetPersonBalanceCommand(PostgresDbContext postgres)
    {
        _postgres = postgres;
    }
    public async ValueTask<GetPersonBalanceResponseDto> Handle(GetPersonBalanceRequestDto request, CancellationToken ct)
    {
        var res = await _postgres.Transactions
            .Where(x => x.PersonId == request.PersonId)
            .GroupBy(x=> x.BankId)
            .Select(x=> new BalanceResponseItemDto(x.Key, x.Sum(y=>y.BonusSum)))
            .ToArrayAsync(ct);
        return new GetPersonBalanceResponseDto(res);
    }
}


public sealed class GetBalanceByBankIdDtoValidator : AbstractValidator<GetBalanceByBankIdDto>
{
    public GetBalanceByBankIdDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.BankId).GreaterThan(0);
    }
}

///  GetBalanceByBankId
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
[Authorize]
[Route("/api/[controller]/[action]")]
public sealed class BalanceController : ControllerBase
{
    /// <summary>
    /// Получить баланс пользователя по всем валютам
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PolicyNames.BalanceRead)]
    public async Task<GetPersonBalanceResponseDto> GetAll([FromServices]IMediator mediator, [FromQuery]GetPersonBalanceRequestDto request)
    {
        var res = await mediator.Send(request);
        return res;
    }

    /// <summary>
    /// Получить баланс пользователя по конкретной валюте
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PolicyNames.BalanceRead)]
    public async Task<long> Get([FromServices]IMediator mediator, [FromQuery]GetBalanceByBankIdDto data)
    {
        var res = await mediator.Send(data);
        return res;
    }
}
