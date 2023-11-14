using BonusService.Postgres;
using FluentValidation;
using Mediator;
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

[ApiController]
[Route("/api/[controller]")]
public sealed class BalanceController : ControllerBase
{
    /// <summary>
    /// Получить баланс пользователя по всем валютам
    /// </summary>
    [HttpGet]
    public async Task<GetPersonBalanceResponseDto> GetPersonBalance([FromServices]IMediator mediator, [FromQuery]GetPersonBalanceRequestDto request)
    {
        var res = await mediator.Send(request);
        return res;
    }
}
