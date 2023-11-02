namespace BonusService.Bonuses;

public record AutoBonusTransaction(Guid PersonId, int BonusSum, int BankId, string Description, int ProgramId);
public record ManualBonusTransaction(Guid PersonId, int BonusSum, int BankId, string Description, Guid TransactionId, Guid UserId);

public interface IBonusService
{
    public Task AutoAccrualAsync(AutoBonusTransaction transaction);
    public Task ManualAccrualAsync(ManualBonusTransaction transaction);
}

public class BonusService : IBonusService
{

    public Task AutoAccrualAsync(AutoBonusTransaction transaction) => throw new NotImplementedException();

    public Task ManualAccrualAsync(ManualBonusTransaction transaction) => throw new NotImplementedException();
}
