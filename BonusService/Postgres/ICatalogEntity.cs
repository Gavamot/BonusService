namespace BonusService.Postgres;

#pragma warning disable CS8618
/// <summary>
/// Интерфейс для справочников
/// </summary>
public interface ICatalogEntity : IHaveId, IHaveDateOfChange
{
    string Name { get; set; }
}

public interface IDeletable
{
    bool IsDeleted { get; set; }
}

public interface IHaveId
{
    int Id { get; set; }
}

public interface IHaveDateOfChange
{
    DateTime LastUpdated { get; set; }
}
