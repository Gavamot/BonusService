using System.ComponentModel.DataAnnotations;
using BonusService.Auth.Policy;
using BonusService.Common.Postgres;
using FluentValidation;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BonusService.Balance.GetBalance;

public sealed class GetPersonBalanceRequestDtoValidator : AbstractValidator<GetPersonBalanceRequest>
{
    public GetPersonBalanceRequestDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
    }
}

public sealed class GetBalanceByBankIdDtoValidator : AbstractValidator<GetBalanceByBankIdRequest>
{
    public GetBalanceByBankIdDtoValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty();
        RuleFor(x => x.BankId).NotEmpty().GreaterThan(0);
    }
}


[ApiController]
[Authorize]
[Route("[controller]/[action]")]
public sealed partial class BalanceController : ControllerBase
{
    /// <summary>
    /// Получить баланс пользователя по всем валютам
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PolicyNames.BalanceRead)]
    public async Task<GetPersonBalanceResponseDto> GetAll([FromServices]IMediator mediator, [FromQuery][Required]GetPersonBalanceRequest request)
    {
        var res = await mediator.Send(request);
        return res;
    }

    /// <summary>
    /// Получить баланс пользователя по конкретной валюте
    /// </summary>
    [HttpGet]
    [Authorize(Policy = PolicyNames.BalanceRead)]
    public async Task<long> Get([FromServices]IMediator mediator, [FromQuery][Required]GetBalanceByBankIdRequest data)
    {
        var res = await mediator.Send(data);
        return res;
    }
}
