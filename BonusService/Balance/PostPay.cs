using System.ComponentModel.DataAnnotations;
using BonusService.Auth.Policy;
using BonusService.Balance.PayManual;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Riok.Mapperly.Abstractions;

// ReSharper disable once CheckNamespace
namespace BonusService.Balance.Pay;

public sealed class PayDtoValidator : AbstractValidator<PayRequestDto>
{
    public PayDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.Payment).NotEmpty().GreaterThan(0).WithMessage("Сумма должна быть положительной");
        RuleFor(x => x.BankId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.TransactionId).NotEmpty();
        RuleFor(x => x.EzsId).NotEmpty();
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}

[Mapper]
public sealed partial class PayDtoMapper
{
    public partial Transaction FromDto(PayRequestDto requestDto);
}

public sealed record PayRequestDto([Required]string PersonId, [Required]int BankId, [Required]long Payment, [Required]string Description, [Required]string TransactionId, [Required]Guid EzsId, [Required]int OwnerId, [property: JsonIgnore]string UserName = "") : ICommand<long>;

public sealed class PayCommand : ICommandHandler<PayRequestDto, long>
{
    private readonly BonusDbContext _bonus;
    private readonly IMediator _mediator;
    private readonly ILogger<PayCommand> _logger;
    public PayCommand(BonusDbContext bonus, IMediator mediator, ILogger<PayCommand> logger)
    {
        _bonus = bonus;
        _mediator = mediator;
        _logger = logger;
    }

    public async ValueTask<long> Handle(PayRequestDto command, CancellationToken ct)
    {
        Transaction transaction = new PayDtoMapper().FromDto(command);
        transaction.Type = TransactionType.Payment;
        var owner = await _bonus.OwnerMaxBonusPays.FirstOrDefaultAsync(x => x.OwnerId == command.OwnerId, ct);
        var percentages = owner?.MaxBonusPayPercentages ?? 100;
        var bonusSum = (command.Payment * percentages) / 100;
        transaction.BonusSum = bonusSum;
        _logger.LogInformation("Запрос на списание бонусов PersonId={PersonId} , BankId={BankId}, сумма платежа = {Payment}, максимальный размер для OwnerId={Percentages}%, сумма бонусов {bonusSum}",
            transaction.PersonId, transaction.BankId,command.Payment, percentages, bonusSum);
        var res = await _mediator.Send(new PayTransactionRequest(transaction), ct);
        return res;
    }
}

[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public sealed partial class BalanceController : ControllerBase
{
    /// <summary>
    /// Списание бонусных баллов сервисом оплаты
    /// </summary>
    [HttpPost]
    [Authorize(Policy = PolicyNames.BonusServiceExecute)]
    public async Task<long> Pay([FromServices]IMediator mediator, [FromBody][Required]PayRequestDto request, CancellationToken ct)
    {
        request = request with { UserName = HttpContext.GetUserName() };
        long res = await mediator.Send(request, ct);
        return res;
    }
}
