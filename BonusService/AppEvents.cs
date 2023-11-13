namespace BonusService.Bonuses;

public static class AppEvents
{
    public readonly static  EventId TransactionShrinkerEvent = new (1, nameof(TransactionShrinkerJob));
}
