namespace BonusService.Postgres;

#pragma warning disable CS8618
public class BonusProgramHistory : IHaveId<int>
{
    public int Id { get; set; }
    public BonusProgram BonusProgram { get; set; }
    public int BonusProgramId { get; set; }
    public DateTimeOffset StartPeriod { get; set; }
    public DateTimeOffset EndPeriod { get; set; }
    public int BankId { get; set; }
    public long TotalSum { get; set; }
    public int ClientAccountsCount { get; set; }
    public long DurationMilliseconds { get; set; }
}
