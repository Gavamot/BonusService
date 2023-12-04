using System.ComponentModel.DataAnnotations;
namespace BonusService.Common.Postgres;

#pragma warning disable CS8618


public interface IDbEntity<T> : IHaveId<T>, IHaveDateOfChange { }

/// <summary>
/// Расширение справочников из других систем
/// </summary>
public interface IForeignCatalogEntity : IDbEntity<int>
{
    // В реализации будут поля для ключа в сторонней базе и какието расширения
}

/// <summary>
///  Справочники
/// </summary>
public interface ICatalogEntity : IDbEntity<int>
{
    [Required]
    string Name { get; set; }
}

/// <summary>
/// Регулярно повторяющиеся документы
/// </summary>
public interface IDocumentEntity : IDbEntity<long> { }

public interface IHaveId<T>
{
    public T Id { get; set; }
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
    public string PersonId { get; }
    public int BankId { get; }
}

record BalanceKey(string PersonId, int BankId) : IBalanceKey;
