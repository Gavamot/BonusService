using System.ComponentModel.DataAnnotations;
using BonusService.Common.Postgres;
using Mediator;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace BonusService.Balance.GetBalance;


public record BalanceResponseItemDto([Required]int BankId, [Required]long Sum);
public sealed record GetBalanceByBankIdRequest([Required]string PersonId, [Required]int BankId) : IRequest<long>, IBalanceKey;

public sealed class GetBalanceByBankIdCommand : IRequestHandler<GetBalanceByBankIdRequest, long>
{
    private readonly PostgresDbContext _postgres;
    public GetBalanceByBankIdCommand(PostgresDbContext postgres)
    {
        _postgres = postgres;
    }
    public async ValueTask<long> Handle(GetBalanceByBankIdRequest request, CancellationToken ct)
    {
        var res = await _postgres.Transactions
            .Where(x => x.PersonId == request.PersonId && x.BankId == request.BankId)
            .SumAsync(x=> x.BonusSum, ct);
        return res;
    }
}
public record GetPersonBalanceRequest([Required]string PersonId) : IRequest<GetPersonBalanceResponseDto>;
public record GetPersonBalanceResponseDto([Required]BalanceResponseItemDto[] items);

public sealed class GetPersonBalanceCommand : IRequestHandler<GetPersonBalanceRequest, GetPersonBalanceResponseDto>
{
    private readonly PostgresDbContext _postgres;
    public GetPersonBalanceCommand(PostgresDbContext postgres)
    {
        _postgres = postgres;
    }
    public async ValueTask<GetPersonBalanceResponseDto> Handle(GetPersonBalanceRequest request, CancellationToken ct)
    {
        var res = await _postgres.Transactions
            .Where(x => x.PersonId == request.PersonId)
            .GroupBy(x=> x.BankId)
            .Select(x=> new BalanceResponseItemDto(x.Key, x.Sum(y=>y.BonusSum)))
            .ToArrayAsync(ct);
        return new GetPersonBalanceResponseDto(res);
    }
}
