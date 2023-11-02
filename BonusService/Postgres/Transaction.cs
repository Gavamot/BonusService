#nullable enable

namespace BonusService.Postgres;

public class Transaction
{
    /// <summary>
    /// Также является ключом идемпотентности
    ///
    /// </summary>
    public Guid Id { get; set; }
    public int PersonId { get; set; }
    public DateTime LastUpdated { get; set; }
    /// <summary>
    /// Тип валюты
    /// </summary>
    public int BankId { get; set; }
    /// <summary>
    ///  База (например денежная сумма) с которой был начислен бонус 0 для ручных начислений
    /// </summary>
    public long BonusBase { get; set; }
    public int BonusSum { get; set; }
    public int? ProgramId { get; set; }
    public virtual Program? Program { get; set; }
    /// <summary>
    /// Id оператора который произвел начисления в случаи если null то начисленно автоматом
    /// </summary>
    public Guid? UserId { get; set; }

    public string Description { get; set; } = "";
}
