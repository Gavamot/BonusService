using System.Text;
using System.Text.Json.Serialization;
using BonusService;
using BonusService.Auth;
using BonusService.BonusPrograms;
using BonusService.Common;
using BonusService.Common.Postgres;
using BonusService.Common.Postgres.Entity;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using NLog.Web;


// TOKEN -
// Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiQWRtaW4iLCJSb2xlcyI6IkFkbWluIiwibmJmIjoxNzAxOTM5MTI0LCJleHAiOjE3OTM0NzUxMjQsImlhdCI6MTcwMTkzOTEyNCwiaXNzIjoiUGxhdGZvcm1XZWJBcGkifQ.7OzTUP5ScmLWtoyCXnYqxWIPJcNuBFweMxep9WKf1BluKyP-i2ZvAcYXPaIRuQdbcc21ghvAMn6UFmQqvBw6Qg
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine($"Environment = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var urls= configuration.GetSection("Urls").Value;
Console.WriteLine($"Running address is urls={urls}/swagger");

var services = builder.Services;

services.AddCors(options =>
{
    options.AddPolicy("AllowAllHeaders",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

//builder.Logging.ClearProviders();
//services.AddCorrelate(options => options.RequestHeaders = new []{ "X-Correlation-ID" });
builder.Logging.ClearProviders();
services.AddLogging(opt =>
{
    opt.Configure(options =>
    {
        options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId | ActivityTrackingOptions.ParentId;
    });
});
builder.Host.UseNLog();
services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode; //HttpLoggingFields.RequestQuery | HttpLoggingFields.RequestHeaders | HttpLoggingFields.RequestBody | HttpLoggingFields.ResponseBody | HttpLoggingFields.ResponseStatusCode;
    logging.RequestHeaders.Add(HeaderNames.Authorization);
    logging.RequestBodyLogLimit = 64 * 1024;
    logging.ResponseBodyLogLimit = 64 * 1024;
});

services.AddHealthChecks();
services.TryAddSingleton<IDateTimeService, DateTimeService>();
services.AddPostgres(configuration);
services.AddMongoService(configuration);

services.AddHangfireService(configuration);

services.AddFluentValidationAutoValidation();
services.AddFluentValidationClientsideAdapters();
services.AddValidatorsFromAssemblyContaining<BonusService.Program>();

services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
}).AddOData(opt =>

    opt.Filter().Select().Expand().SetMaxTop(100).Count().OrderBy().EnableQueryFeatures());
    /*opt.AddRouteComponents("api/odata/transaction", GetEdmModel())
        .Filter().Select().Expand().SetMaxTop(100).Count().OrderBy().EnableQueryFeatures()
    );*/



services.AddBonusServices(configuration);

services.AddJwtAuthorization(configuration);
services.AddAppSwagger();
services.AddMediator(opt =>
{
    opt.ServiceLifetime = ServiceLifetime.Scoped;
});

WebApplication app = builder.Build();

app.UseCors("AllowAllHeaders");

app.UseHealthChecks("/healthz");
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHangfire();
app.UseAppSwagger();

app.UseHttpLogging();
app.UseRouting();

app.UseJwtAuthorization();

app.UseHttpsRedirection();
app.MapControllers();

app.UseODataRouteDebug();

app.ApplyPostgresMigrations();

if (BonusService.Program.IsNotAppTest())
{
    using var scope = app.Services.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<IBonusProgramsRunner>();
    await runner.RestartAsync();
}

app.Run();

static IEdmModel GetEdmModel()
{
    ODataConventionModelBuilder builder = new();
    var entitySet = builder.EntitySet<Transaction>("transaction");
    entitySet.EntityType.HasKey(entity => entity.Id);
    return builder.GetEdmModel();
}

namespace BonusService
{
    public partial class Program
    {
        public const string AppTest = nameof(AppTest);

        public static bool IsAllowDisableAuth() => IsNswagBuild() || IsAppTest();
        public static bool IsAppTest() => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(AppTest)) == false;
        public static bool IsNotAppTest() => !IsAppTest();
        public static bool IsLocal() => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Local"
            || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "DevRemote";
        public static bool IsNotLocal() => !IsLocal();
        public static bool IsNswagBuild() => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NswagGen")) == false;
        public static bool IsNotNswagBuild() => !IsNswagBuild();
    }
}
