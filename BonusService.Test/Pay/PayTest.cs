using BonusApi;
using BonusService.Postgres;
using BonusService.Test.Common;
using FluentAssertions;

namespace BonusService.Test;

public class PayTest : BonusTestApi
{
    public PayTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    [Fact]
    public async Task ZeroBalance_ZeroBonusPay()
    {
        var payed = await api.ApiPayAsync(new PayRequestDto()
        {
            Description = Q.Description1,
            TransactionId = Q.GetRandomTransactionId(),
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            OwnerId = Q.OwnerId1,
            EzsId = Q.EzsId1,
            Payment = 500
        });
        payed.Should().Be(0);
    }

    [Fact]
    public async Task HavePositiveBalanceForAnotherBalancesButPayedIsZero_ZeroBonusPay()
    {
        await postgres.Transactions.AddRangeAsync(new []
        {
            Q.CreateTransaction(Q.PersonId1),
            Q.CreateTransaction(Q.PersonId1),
            Q.CreateTransaction(Q.PersonId2),

        });
        await postgres.SaveChangesAsync();
        var payed = await api.ApiPayAsync(new PayRequestDto()
        {
            Description = Q.Description1,
            TransactionId = Q.GetRandomTransactionId(),
            BankId = Q.BankIdKaz,
            PersonId = Q.PersonId1,
            OwnerId = Q.OwnerId1,
            EzsId = Q.EzsId1,
            Payment = 500
        });
        payed.Should().Be(0);
    }

    [Fact]
    public async Task HaveEnoughSumOnDifferentBalancesButNotEnoughOnPayBalance_PayAllFromPayBalanceOnly()
    {
        await postgres.Transactions.AddRangeAsync(new []
        {
            Q.CreateTransaction(Q.PersonId1, Q.BankIdRub, Q.Sum1000),
            Q.CreateTransaction(Q.PersonId1, Q.BankIdKaz, Q.Sum1000),
            Q.CreateTransaction(Q.PersonId2, Q.BankIdRub, Q.Sum1000),
        });
        await postgres.SaveChangesAsync();
        var payed = await api.ApiPayAsync(new PayRequestDto()
        {
            Description = Q.Description1,
            TransactionId = Q.TransactionId1,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            OwnerId = Q.OwnerId1,
            EzsId = Q.EzsId1,
            Payment = Q.Sum2000
        });
        payed.Should().Be(Q.Sum1000);

        var transaction = postgres.Transactions.Single(x => x.BonusSum == Q.Sum1000 * -1);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.PersonId.Should().Be(Q.PersonId1);
        transaction.OwnerId.Should().Be(Q.OwnerId1);
        transaction.TransactionId.Should().Be(Q.TransactionId1);
        transaction.Description.Should().Be(Q.Description1);
        transaction.Type.Should().Be(TransactionType.Payment);
        transaction.BankId.Should().Be(Q.BankIdRub);
        transaction.EzsId.Should().Be(Q.EzsId1);
        transaction.UserId.Should().BeNull();
        transaction.BonusProgramId.Should().Be(0);
        transaction.BonusBase.Should().BeNull();
        transaction.LastUpdated.Should().Be(Q.DateTimeSequence.First());
    }

    [Fact]
    public async Task HaveMoreSumBalanceRub_PayRubBalanceOnlyStayBonusesBeforePay()
    {
        await postgres.Transactions.AddRangeAsync(new []
        {
            Q.CreateTransaction(Q.PersonId1, Q.BankIdRub, Q.Sum1000),
            Q.CreateTransaction(Q.PersonId1, Q.BankIdKaz, Q.Sum1000),
            Q.CreateTransaction(Q.PersonId2, Q.BankIdRub, Q.Sum1000),
        });
        await postgres.SaveChangesAsync();
        var payed = await api.ApiPayAsync(new PayRequestDto()
        {
            Description = Q.Description1,
            TransactionId = Q.TransactionId1,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            OwnerId = Q.OwnerId1,
            EzsId = Q.EzsId1,
            Payment = Q.Sum500
        });
        payed.Should().Be(Q.Sum500);

        var rest = postgres.Transactions.Where(x=> x.PersonId == Q.PersonId1 && x.BankId == Q.BankIdRub).Sum(x=> x.BonusSum);
        rest.Should().Be(Q.Sum500);
    }

    [Fact]
    public async Task PayBonusesWithOwnerDoNotPay_ReturnZeroNoPayTransactions()
    {
        postgres.Transactions.AddRange(new []
        {
            Q.CreateTransaction(Q.PersonId1, Q.BankIdRub, Q.Sum1000),
            Q.CreateTransaction(Q.PersonId1, Q.BankIdKaz, Q.Sum1000),
            Q.CreateTransaction(Q.PersonId2, Q.BankIdRub, Q.Sum1000),
        });

        postgres.OwnerMaxBonusPays.AddRange(new (){
            OwnerId = Q.OwnerId2,
            MaxBonusPayPercentages = 10
        },
            new ()
        {
            OwnerId = Q.OwnerId1,
            MaxBonusPayPercentages = 0
        }, new ()
            {
                OwnerId = 3,
                MaxBonusPayPercentages = 50
            });

        await postgres.SaveChangesAsync();
        var payed = await api.ApiPayAsync(new PayRequestDto()
        {
            Description = Q.Description1,
            TransactionId = Q.TransactionId1,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            OwnerId = Q.OwnerId1,
            EzsId = Q.EzsId1,
            Payment = Q.Sum500
        });
        payed.Should().Be(0);

        var isPayedTransactionExist = postgres.Transactions.Any(x=> x.BonusSum <= 0);
        isPayedTransactionExist.Should().BeFalse();
    }

    [Theory]
    [InlineData(1, 100, 99, 0)]
    [InlineData(2, 100, 30, 0)]
    [InlineData(10, 100, 5, 0)]
    public async Task NotEnoughToPayWithPercentages_MustReturnZeroNoAnyTransactions(long payX, long BalanceY, int PercentagesZ, long SpendN)
    {
        postgres.Transactions.AddRange(new []
        {
            Q.CreateTransaction(Q.PersonId1, Q.BankIdRub, BalanceY),
            Q.CreateTransaction(Q.PersonId1, Q.BankIdKaz, Q.Sum1000),
            Q.CreateTransaction(Q.PersonId2, Q.BankIdRub, Q.Sum1000),
        });

        postgres.OwnerMaxBonusPays.AddRange(new()
            {
                OwnerId = Q.OwnerId2,
                MaxBonusPayPercentages = 10
            },
            new()
            {
                OwnerId = Q.OwnerId1,
                MaxBonusPayPercentages = PercentagesZ
            }, new()
            {
                OwnerId = 3,
                MaxBonusPayPercentages = 50
            });

        await postgres.SaveChangesAsync();
        var payed = await api.ApiPayAsync(new PayRequestDto()
        {
            Description = Q.Description1,
            TransactionId = Q.TransactionId1,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            OwnerId = Q.OwnerId1,
            EzsId = Q.EzsId1,
            Payment = payX
        });
        payed.Should().Be(SpendN);

        var spendTransactions = postgres.Transactions.Where(x => x.BonusSum <= 0).Sum(x => x.BonusSum);
        spendTransactions.Should().Be(SpendN);
        postgres.Transactions.Any(x => x.BonusSum == SpendN*-1).Should().BeFalse();
    }

    [Theory]
    [InlineData(1000, 2000, 100, 1000)]
    [InlineData(1000, 1, 20, 1)]
    [InlineData(1000, 500, 20, 200)]
    public async Task PayFromSumXPersonHasYBonusesWithZPercentages_MustPayN(long payX, long BalanceY, int PercentagesZ, long SpendN)
    {
        postgres.Transactions.AddRange(new []
        {
            Q.CreateTransaction(Q.PersonId1, Q.BankIdRub, BalanceY),
            Q.CreateTransaction(Q.PersonId1, Q.BankIdKaz, Q.Sum1000),
            Q.CreateTransaction(Q.PersonId2, Q.BankIdRub, Q.Sum1000),
        });

        postgres.OwnerMaxBonusPays.AddRange(new()
            {
                OwnerId = Q.OwnerId2,
                MaxBonusPayPercentages = 10
            },
            new()
            {
                OwnerId = Q.OwnerId1,
                MaxBonusPayPercentages = PercentagesZ
            }, new()
            {
                OwnerId = 3,
                MaxBonusPayPercentages = 50
            });

        await postgres.SaveChangesAsync();

        var payed = await  api.ApiPayAsync(new PayRequestDto()
        {
            Description = Q.Description1,
            TransactionId = Q.TransactionId1,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            OwnerId = Q.OwnerId1,
            EzsId = Q.EzsId1,
            Payment = payX
        });
        payed.Should().Be(SpendN);

        var spendTransactions = postgres.Transactions.Where(x => x.BonusSum <= 0).Sum(x => x.BonusSum);
        spendTransactions.Should().Be(SpendN*-1);
        var transaction = postgres.Transactions.Single(x => x.BonusSum == SpendN*-1);
        transaction.OwnerId.Should().Be(Q.OwnerId1);
        transaction.PersonId.Should().Be(Q.PersonId1);
        transaction.EzsId.Should().Be(Q.EzsId1);
    }

     [Fact]
    public async Task WrongParameters_TrowsException()
     {
         void PayTrows(PayRequestDto request)
         {
             Assert.ThrowsAsync<ApiException>(async () =>
            {
                 await api.ApiPayAsync(request);
            }).GetAwaiter().GetResult();
        }

        var requestOriginal = new PayRequestDto()
        {
            Description = Q.Description1,
            Payment = Q.Sum1000,
            BankId = Q.BankIdRub,
            PersonId = Q.PersonId1,
            TransactionId = Q.TransactionId1,
            OwnerId = Q.OwnerId1,
            EzsId = Q.EzsId1
        };

        var request = requestOriginal.ToJsonClone();
        request.Description = "";
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.Description = null;
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.Description = " "; // Tab
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.Description = "  "; // Spaces
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.Payment = 0;
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.Payment = -1;
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.BankId = 0;
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.BankId = -1;
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.PersonId = default;
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.TransactionId = "";
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.TransactionId = null;
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.OwnerId = default;
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.OwnerId = 0;
        PayTrows(request);

        request = requestOriginal.ToJsonClone();
        request.EzsId = default;
        PayTrows(request);
    }
}
