namespace BonusService.Postgres;

/// <summary>
/// Регистр остатки бонусов по счету пользователя
/// </summary>
public class BalanceRegister : IDocumentEntity, IBalanceKey
{
    public long Id { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public Guid PersonId { get; set; }

    public long Sum { get; set; }
    /// <summary>
    /// Тип валюты
    /// </summary>
    public int BankId { get; set; }
    public DateTimeOffset Date { get; set; }
}
