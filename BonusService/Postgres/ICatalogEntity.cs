namespace BonusService.Postgres;

#pragma warning disable CS8618
/// <summary>
/// Интерфейс для справочников
/// </summary>
public interface ICatalogEntity : IHaveId, IHaveDateOfChange
{
    public string Name { get; set; }
}

public interface IHaveId
{
    public int Id { get; set; }
}

public interface IHaveDateOfChange
{
    public DateTime LastUpdated { get; set; }
}
