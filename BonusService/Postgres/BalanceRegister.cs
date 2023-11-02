namespace BonusService.Postgres;

/// <summary>
/// Регистр остатки бонусов по счету пользователя
/// </summary>
public class BalanceRegister : IHaveId, IHaveDateOfChange
{
    public int Id { get; set; }
    public DateTime LastUpdated { get; set; }
    public int PersonId { get; set; }

    public long Sum { get; set; }
    /// <summary>
    /// Тип валюты
    /// </summary>
    public int BankId { get; set; }
}
