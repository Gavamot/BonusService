namespace BonusService.Postgres;

public class OwnerMaxBonusPay : IForeignCatalogEntity
{
    public int Id { get; set; }
    /// <summary>
    /// ID owner в другой базе данных
    /// </summary>
    public int OwnerId { get; set; }
    /// <summary>
    /// Максимальный размер оплаты бонусами в процентах
    /// </summary>
    public int MaxBonusPayPercentages { get; set; }

    public DateTimeOffset LastUpdated { get; set; }
}
