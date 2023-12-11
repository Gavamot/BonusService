namespace ImportBonuse.Postgres;

public interface IDbEntity<T> : IHaveId<T>, IHaveDateOfChange { }


/// <summary>
/// Регулярно повторяющиеся документы
/// </summary>
public interface IDocumentEntity : IDbEntity<long> { }

public interface IHaveId<T>
{
    public T Id { get; set; }
}

public interface IHaveDateOfChange
{
    DateTimeOffset LastUpdated { get; set; }
}

public interface IBalanceKey
{
    public string PersonId { get; }
    public int BankId { get; }
}
