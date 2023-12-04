using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// ReSharper disable once CheckNamespace
namespace BonusService.Balance.BalanceTransactions;

public record class BalanceTransactionResponse
{
    public BalanceTransactionDto[] Items { get; set; }
    public int Count { get; }
}

public record class BalanceTransactionDto
{
    public DateTimeOffset LastUpdated { get; set; }
    public long BonusSum { get; set; }
    public string Description { get; set; }
}

public record class BalanceTransactionRequest(string PersonId, int BankId, int Skip, int Take) : IRequest<BalanceTransactionResponse>;

/// <summary>
/// Получчение истории начисления/списания бонусов
/// </summary>
[ApiController]
[Authorize]
[Route("/[controller]/[action]")]
public sealed class BalanceController : ControllerBase
{
    public BalanceController()
    {

    }

    [HttpGet]
    public Task<BalanceTransactionResponse> Transactions([FromQuery]BalanceTransactionRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
