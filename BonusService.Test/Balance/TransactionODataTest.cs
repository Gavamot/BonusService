using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BonusApi;
using BonusService.Test.Common;
using Microsoft.AspNetCore.Http;
namespace BonusService.Test;

public class TransactionODataTest : BonusTestApi
{
    public TransactionODataTest(FakeApplicationFactory<Program> server) : base(server)
    {
    }

    [Fact]
    public async Task EmptyBonuses_ReturnEmptyList()
    {
        HttpClient http = CreateHttpClient();
        var res = await http.GetAsync("api/transaction");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var data =await res.Content.ReadFromJsonAsync<Transaction[]>();
        data.Should().BeEmpty();

    }
}
