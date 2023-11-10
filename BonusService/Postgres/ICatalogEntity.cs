namespace BonusService.Postgres;

#pragma warning disable CS8618
/// <summary>
/// Интерфейс для справочников
/// </summary>
public interface ICatalogEntity : IHaveDateOfChange
{
    int Id { get; set; }
    string Name { get; set; }
}

public interface IDocumentEntity : IHaveDateOfChange
{
    long Id { get; set; }
}

public interface IDeletable
{
    bool IsDeleted { get; set; }
}

public interface IHaveDateOfChange
{
    DateTimeOffset LastUpdated { get; set; }
}

public interface IBalanceKey
{
    public Guid PersonId { get; }
    public int BankId { get; }
}

record BalanceKey(Guid PersonId, int BankId) : IBalanceKey;
