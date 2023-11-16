using BonusApi;
using BonusService.Postgres;
using BonusService.Test.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace BonusService.Test;

public class ManualAccrualTest : BonusTestApi
{
    public ManualAccrualTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    [Fact]
    public async Task PositiveCase()
    {
        long sum = 1000L;
        var request = new AccrualManualRequestDto()
        {
            Description = Q.Description1,
            BonusSum = sum,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId1,
            UserId = Q.UserId1
        };
        await api.ApiAccrualManualAsync(request);
        using var scope = Server.Services.CreateScope();
        var postgres = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
        var transaction = await postgres.Transactions.SingleAsync();
        transaction.TransactionId.Should().Be(Q.TransactionId1);
        transaction.Description.Should().Be(Q.Description1);
        transaction.Type.Should().Be(TransactionType.Manual);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.BonusSum.Should().Be(sum);
        transaction.UserId.Should().Be(Q.UserId1);

        transaction.EzsId.Should().BeNull();
        transaction.BonusProgramId.Should().BeNull();
        transaction.BonusBase.Should().BeNull();
    }

}
