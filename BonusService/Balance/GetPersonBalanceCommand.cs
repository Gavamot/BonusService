using System.ComponentModel.DataAnnotations;
using BonusService.Common.Postgres;
using Mediator;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace BonusService.Balance.GetBalance;

public record GetPersonBalanceRequestDto([Required]string PersonId) : IRequest<GetPersonBalanceResponseDto>;
public record GetPersonBalanceResponseDto([Required]BalanceResponseItemDto[] items);
public record BalanceResponseItemDto([Required]int BankId, [Required]long Sum);
public sealed record GetBalanceByBankIdDto([Required]string PersonId, [Required]int BankId) : IRequest<long>, IBalanceKey;

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
