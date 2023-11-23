#nullable enable

using Mediator;
namespace BonusService.Postgres;

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
    Shrink
}

public class Transaction : IDocumentEntity, IBalanceKey
{
    public long Id { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    /// <summary>
    /// Тип валюты
    /// </summary>
    public Guid PersonId { get; set; }
    public int BankId { get; set; }

    /// <summary>
    ///  База (например денежная сумма) с которой был начислен бонус 0 для ручных начислений
    /// </summary>
    public long? BonusBase { get; set; }
    public long BonusSum { get; set; }
    public int? BonusProgramId { get; set; }
    //public virtual BonusProgram? Program { get; set; }
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
