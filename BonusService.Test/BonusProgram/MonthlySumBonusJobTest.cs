using BonusApi;
using BonusService.Bonuses;
using BonusService.Test.Common;
using Hangfire;
using Hangfire.Storage.Monitoring;
using BonusProgram = BonusService.Postgres.BonusProgram;
namespace BonusService.Test;

public class MonthlySumBonusJobTest : BonusTestApi
{
    private BonusProgram bonusProgram = new BonusProgramRep().Get();
    private string jobId => $"bonusProgram_{bonusProgram.Id}";
    public MonthlySumBonusJobTest(FakeApplicationFactory<Program> server) : base(server)
    {

    }

    [Fact]
    public async Task EmptyCalculation_EmptyResult()
    {
        jobManager.AddOrUpdate<MonthlySumBonusJob>(jobId, x=> x.ExecuteAsync(bonusProgram), Cron.Never);
        var job = jobManager.TriggerJob(jobId);
        var details = jobManager.Storage.GetMonitoringApi().JobDetails(job);
        var a = 1;
    }
}
