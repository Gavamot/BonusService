#nullable enable

using System.Runtime.Serialization;
using BonusService.Common.Swagger;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
#pragma warning disable CS8618
namespace BonusService.Common.Postgres.Entity;

public enum TransactionType
{
    /// <summary>
    /// Начислено/Списано оператором через админку
    /// </summary>
    Manual,

    /// <summary>
    /// Начислено/Списано При расчете за услугу
    /// </summary>
    Payment,

    /// <summary>
    /// Начислено/Списано Сервисом бонус автоматически (переодическое начисление, сгорели бонусы)
    /// </summary>
    Auto,

    /// <summary>
    /// Консолидация оборотов по бонусному счету
    /// </summary>
    Shrink,

    /// <summary>
    /// По событию (например день регистрация нового пользователя)
    /// </summary>
    Event
}

public class Transaction : IDocumentEntity, IBalanceKey
{
    public long Id { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    /// <summary>
    /// Тип валюты
    /// </summary>
    public string PersonId { get; set; }
    public int BankId { get; set; }

    /// <summary>
    ///  База (например денежная сумма) с которой был начислен бонус 0 для ручных начислений
    /// </summary>
    public long? BonusBase { get; set; }
    public long BonusSum { get; set; }
    public int? BonusProgramId { get; set; }

    [JsonIgnore]
    [SwaggerSchema(ReadOnly = true)]
    [SwaggerExclude]
    public BonusProgram? BonusProgram { get; set; } = null;
    /// <summary>
    /// Id оператора который произвел начисления в случаи если null то начисленно автоматом
    /// </summary>
    public string? UserName { get; set; }
    public string Description { get; set; } = "";

    /// <summary>
    /// Заправка при расплате за которую списались бонусы. Если null то происходило списание в ручную либо по иным причинам например сгорели бонусы.
    /// </summary>
    public Guid? EzsId { get; set; }

    public int? OwnerId { get; set; }

    public TransactionType Type { get; set; }
    public string TransactionId { get; set; }
}
