using System.Security.Cryptography;
using BonusApi;
using BonusService.Common.Postgres;
using BonusService.Test.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OwnerMaxBonusPay = BonusService.Common.Postgres.Entity.OwnerMaxBonusPay;
namespace BonusService.Test;

public class OwnerMaxBonusPayCrudTest : BonusTestApi
{
    public OwnerMaxBonusPayCrudTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    [Fact]
    public async Task GetById()
    {
        postgres.OwnerMaxBonusPays.AddRange(new OwnerMaxBonusPay[]
        {
            new(){ OwnerId = Q.OwnerId1, MaxBonusPayPercentages = 10},
            new(){ OwnerId = Q.OwnerId2, MaxBonusPayPercentages = 20}
        });
        await postgres.SaveChangesAsync();

        var ow1= await api.OwnerMaxBonusPayGetByIdAsync(1);
        ow1.OwnerId.Should().Be(Q.OwnerId1);
        ow1.MaxBonusPayPercentages.Should().Be(10);
        ow1.Id.Should().BePositive();

        var ow2 = await api.OwnerMaxBonusPayGetByIdAsync(2);
        ow2.OwnerId.Should().Be(Q.OwnerId2);
        ow2.MaxBonusPayPercentages.Should().Be(20);
        ow2.Id.Should().BePositive();
    }

    [Fact]
    public async Task GetByOwnerId()
    {
        postgres.OwnerMaxBonusPays.AddRange(new OwnerMaxBonusPay[]
        {
            new(){ OwnerId = Q.OwnerId1, MaxBonusPayPercentages = 10},
            new(){ OwnerId = Q.OwnerId2, MaxBonusPayPercentages = 20}
        });
        await postgres.SaveChangesAsync();

        var ow1= await api.OwnerMaxBonusPayGetByOwnerIdAsync(Q.OwnerId1);
        ow1.OwnerId.Should().Be(Q.OwnerId1);
        ow1.MaxBonusPayPercentages.Should().Be(10);
        ow1.Id.Should().BePositive();

        var ow2 = await api.OwnerMaxBonusPayGetByOwnerIdAsync(Q.OwnerId2);
        ow2.OwnerId.Should().Be(Q.OwnerId2);
        ow2.MaxBonusPayPercentages.Should().Be(20);
        ow2.Id.Should().BePositive();
    }

    [Fact]
    public async Task GetAll()
    {
        postgres.OwnerMaxBonusPays.AddRange(new OwnerMaxBonusPay[]
        {
            new(){ OwnerId = Q.OwnerId1, MaxBonusPayPercentages = 10},
            new(){ OwnerId = Q.OwnerId2, MaxBonusPayPercentages = 20}
        });
        await postgres.SaveChangesAsync();

        var owners= await api.OwnerMaxBonusPayGetAllAsync();
        owners.Count.Should().Be(2);
        owners.First().Should().NotBeNull();
        owners.Last().Should().NotBeNull();
    }

    [Fact]
    public async Task Add()
    {
        var owner = await api.OwnerMaxBonusPayAddAsync(new BonusApi.OwnerMaxBonusPay()
        {
            OwnerId = Q.OwnerId1,
            MaxBonusPayPercentages = 20
        });

        owner.Id.Should().BePositive();
        owner.OwnerId.Should().Be(Q.OwnerId1);
        owner.MaxBonusPayPercentages.Should().Be(20);
        owner.LastUpdated.Should().Be(Q.DateTimeSequence.First());

        postgres.OwnerMaxBonusPays.Count().Should().Be(1);
        var entity = postgres.OwnerMaxBonusPays.First();
        entity.Id.Should().BePositive();
        entity.OwnerId.Should().Be(Q.OwnerId1);
        entity.MaxBonusPayPercentages.Should().Be(20);
        entity.LastUpdated.Should().Be(Q.DateTimeSequence.First());
    }

    [Fact]
    public async Task Update()
    {
        postgres.OwnerMaxBonusPays.AddRange(new OwnerMaxBonusPay[]
        {
            new(){ OwnerId = Q.OwnerId1, MaxBonusPayPercentages = 10},
            new(){ OwnerId = Q.OwnerId2, MaxBonusPayPercentages = 20}
        });
        await postgres.SaveChangesAsync();

        var id = postgres.OwnerMaxBonusPays.Single(x => x.OwnerId == Q.OwnerId2).Id;

        await api.OwnerMaxBonusPayUpdateAsync(new OwnerByPayDto()
        {
            Id = id,
            MaxBonusPayPercentages = 50
        });

        using var scope = CreateScope();
        var newPostgres = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        newPostgres.OwnerMaxBonusPays.Count().Should().Be(2);
        var updatedOwner = await newPostgres.OwnerMaxBonusPays.FirstAsync(x => x.Id == id);
        updatedOwner.MaxBonusPayPercentages.Should().Be(50);
        updatedOwner.LastUpdated.Should().Be(Q.DateTimeSequence.First());
        updatedOwner.OwnerId.Should().Be(Q.OwnerId2);
    }

    [Fact]
    public async Task Delete()
    {
        var date = new DateTimeOffset(2000, 1, 1, 1, 1, 1, new TimeSpan(0));
        postgres.OwnerMaxBonusPays.AddRange(new OwnerMaxBonusPay[]
        {
            new(){ OwnerId = Q.OwnerId1, MaxBonusPayPercentages = 10, LastUpdated = date},
            new(){ OwnerId = Q.OwnerId2, MaxBonusPayPercentages = 20, LastUpdated = date + new TimeSpan(1, 1, 1, 1)}
        });
        await postgres.SaveChangesAsync();


        var id = postgres.OwnerMaxBonusPays.Single(x => x.OwnerId == Q.OwnerId2).Id;

        await api.OwnerMaxBonusPayDeleteByIdAsync(id);

        using var scope = CreateScope();
        var newPostgres = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();

        newPostgres.OwnerMaxBonusPays.Count().Should().Be(1);
        var updatedOwner = await newPostgres.OwnerMaxBonusPays.FirstAsync(x => x.OwnerId == Q.OwnerId1);
        updatedOwner.MaxBonusPayPercentages.Should().Be(10);
        updatedOwner.LastUpdated.Should().Be(date);
        updatedOwner.Id.Should().BePositive();
    }
}
