#nullable enable
namespace BonusService.Postgres;

public class Transaction
{
    public long Id { get; set; }
    public int PersonId { get; set; }
    public DateTime Created { get; set; }
    public int BankId { get; set; }
    /// <summary>
    ///  База с которой был начислен бонус 0 для ручных начислений
    /// </summary>
    public long BonusBase { get; set; }
    public int BonusSum { get; set; }
    public int? BonusProgramId { get; set; }
    public virtual Program? BonusProgram { get; set; }
    /// <summary>
    /// Id оператора который произвел начисления в случаи если null то начисленно автоматом
    /// </summary>
    public Guid? UserId { get; set; }

    public string Description { get; set; }
}
