using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using BonusService.Auth.Policy;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwaggerExampleAttrLib;
// ReSharper disable once CheckNamespace
namespace BonusService.Balance.BalanceTransactions;

public record BalanceTransactionResponse(int Count, Transaction [] Items);
//public record BalanceTransactionResponse(int Count, BalanceTransactionDto [] Items);
//
// public record class BalanceTransactionDto
// {
//     public long Id { get; set; }
//     public DateTimeOffset Date { get; set; }
//     public long BonusSum { get; set; }
//     public string Description { get; set; } = "";
//     public string TransactionId { get; set; } = "";
//     public Guid? EzsId { get; set; }
//     public int? BonusProgramId { get; set; }
//     public TransactionType Type { get; set; }
//     public BonusProgramType? BonusProgramType { get; set; }
//     public string? BonusProgramName { get; set; }
//     public long? BonusBase { get; set; }
//     public string? UserName { get; set; }
//     public int? OwnerId { get; set; }
// }
//
// [Mapper]
// public partial class BalanceTransactionDtoMapper
// {
//     public BalanceTransactionDto ToDto(Transaction transaction)
//     {
//         // custom before map code...
//         var dto = InnerToDto(transaction);
//         dto.BonusProgramName = transaction.BonusProgram?.Name ?? "";
//         dto.BonusProgramType = transaction.BonusProgram?.BonusProgramType;
//         return dto;
//     }
//
//     [MapProperty(nameof(Transaction.LastUpdated), nameof(BalanceTransactionDto.Date))]
//     private partial BalanceTransactionDto InnerToDto(Transaction transaction);
// }

public sealed class GetBalanceByBankIdDtoValidator : AbstractValidator<BalanceTransactionRequest>
{
    public GetBalanceByBankIdDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.BankId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Limit).GreaterThan(0);
    }
}

public sealed class GetBalanceTransactionCommand : IRequestHandler<BalanceTransactionRequest, BalanceTransactionResponse>
{
    private readonly BonusDbContext _bonus;
    public GetBalanceTransactionCommand(BonusDbContext bonus)
    {
        _bonus = bonus;
    }

    public async ValueTask<BalanceTransactionResponse> Handle(BalanceTransactionRequest request, CancellationToken ct)
    {
        if (request.DateTo == default)
        {
            request = request with { DateTo =  DateOnly.MaxValue};
        }

        if (request.DateTo < request.DateFrom) throw new ArgumentException("Неверно задан интервал");

        var start = request.DateFrom.ToDateTimeOffset();
        var end = request.DateTo.ToDateTimeOffset();
        var dbQuery = _bonus.Transactions.Where(x => x.PersonId == request.PersonId
            && x.BankId == request.BankId
            && x.LastUpdated >= start
            && x.LastUpdated < end);

        var count = await dbQuery.CountAsync(ct);
        //var mapper = new BalanceTransactionDtoMapper();

        var transactions = await dbQuery
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            //.Include(x=>x.BonusProgram)
            //.Select(x=> mapper.ToDto(x))
            .AsNoTracking()
            .ToArrayAsync(ct);

        return new BalanceTransactionResponse(count, transactions);
    }
}

public record class BalanceTransactionRequest([Required]string PersonId,
    [Required][property: Example(1)]int BankId,
    [DefaultValue(1)][property: Example(1)]int Page = 1,
    [DefaultValue(10)][property: Example(10)]int Limit = 10,
    [DefaultValue(null)]DateOnly @DateFrom = default,
    [DefaultValue(null)]DateOnly @DateTo =  default) : IRequest<BalanceTransactionResponse>;

/// <summary>
/// Получчение истории начисления/списания бонусов
/// </summary>
[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public sealed class BalanceController : ControllerBase
{
    /// <summary>
    /// Получение истории начисления/списания бонусов
    /// </summary>
    /// <remarks>
    /// LastUpdated - дата транзакции
    /// string($date) - дата необходимо передавать в формате yyyy-MM-dd примеры: 2023-12-22 , 2023-01-02
    /// Крайняя дата не влючается в выборку например чтобы получить данные весь 1 день нужно передать { DateFrom :2023-12-22, DateTo : 2023-12-23 }
    /// </remarks>
    [HttpGet]
    [Authorize(Policy = PolicyNames.PersonRead)]
    public async Task<BalanceTransactionResponse> GetTransactions([FromServices]IMediator mediator, [FromQuery]BalanceTransactionRequest request, CancellationToken ct)
    {
        var res = await mediator.Send(request, ct);
        return res;
    }
}
