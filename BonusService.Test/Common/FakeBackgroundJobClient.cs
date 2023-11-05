using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.Extensions.DependencyInjection;
namespace BonusService.Test.Common;

public class FakeBackgroundJobClient : IBackgroundJobClient
{
    private readonly IServiceProvider serviceProvider;

    public FakeBackgroundJobClient(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }
    public string Create(Job job, IState state)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            object? instance = null;

            if (!job.Method.IsStatic)
            {
                instance = scope.ServiceProvider.GetService(job.Type);

                if (instance == null)
                {
                    throw new InvalidOperationException(
                        $"JobActivator returned NULL instance of the '{job.Type}' type.");
                }
            }
            var arguments = SubstituteArguments(job);
            var res = job.Method.Invoke(instance, arguments);
            return "1234";
        }

    }

    private static object[] SubstituteArguments(Job job)
    {

        var parameters = job.Method.GetParameters();
        var result = new List<object>(job.Args.Count);

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var argument = job.Args[i];

            var value = argument;
            result.Add(value);
        }

        return result.ToArray();
    }

    public bool ChangeState(string jobId, IState state, string expectedState)
    {
        return true;
    }
}
