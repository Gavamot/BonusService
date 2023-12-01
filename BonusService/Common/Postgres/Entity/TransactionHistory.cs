#pragma warning disable CS8618
namespace BonusService.Common.Postgres.Entity;

public class TransactionHistory
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
