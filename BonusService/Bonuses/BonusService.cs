using BonusService.Postgres;
namespace BonusService.Bonuses;

public record BonusAuto(Guid PersonId, int BankId, int Sum, string Description, string TransactionId, int? ProgramId = null, Guid? EzsId = null);
public record BonusManual(Guid PersonId, int BankId, int Sum, string Description, string TransactionId, Guid UserId);


public interface IBonusService
{
    public Task AccrualAutoAsync(BonusAuto transaction);
    public Task<int> PayAutoAsync(BonusAuto transaction);
    public Task AccrualManualAsync(BonusManual transaction);
    public Task<int> PayManualAsync(BonusManual transaction);
    public Task<long> GetBalanceAsync(IBalanceKey balanceKey);
}

public class BonusService : IBonusService
{
    public Task AccrualAutoAsync(BonusAuto transaction) => throw new NotImplementedException();
    public Task AccrualManualAsync(BonusManual transaction) => throw new NotImplementedException();
    public Task<int> PayAutoAsync(BonusAuto transaction) => throw new NotImplementedException();
    public Task<int> PayManualAsync(BonusManual transaction) => throw new NotImplementedException();
    public Task<long> GetBalanceAsync(IBalanceKey balanceKey) => throw new NotImplementedException();


}
