using System.Text.Json.Serialization;
using BonusService.Auth;
using BonusService.Bonuses;
using BonusService.Common;
using BonusService.Pay;
using BonusService.Postgres;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;
using NLog.Web;


// TOKEN -
// Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJuYW1lIjoiQWRtaW4iLCJSb2xlcyI6IkFkbWluIiwibmJmIjoxNzAwNjUyODQzLCJleHAiOjE3OTA2NTI4NDMsImlhdCI6MTcwMDY1Mjg0MywiaXNzIjoiUGxhdGZvcm1XZWJBcGkifQ.C3zc6s9FH7emLZVpRyaulc_aw2QD4gNzaUNTLXVnj_FDhSQzDxijr7aWYrT3XT2gPziHYUFh8uBtIAY_nfQ3Mw
Console.WriteLine($"Environment = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

var urls= configuration.GetSection("Urls").Value;
Console.WriteLine($"Running address is urls={urls}");

var services = builder.Services;


services.AddScoped<IBonusProgramRep, BonusProgramRep>();
services.AddScoped<OwnerByPayRep>();

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
services.AddValidatorsFromAssemblyContaining<Program>();

services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


services.AddBonusServices(configuration);

services.AddJwtAuthorization(configuration);

services.AddSwagger();

services.AddMediator(opt =>
{
    opt.ServiceLifetime = ServiceLifetime.Scoped;
});

WebApplication app = builder.Build();

app.UseCors("AllowAllHeaders");

app.UseHealthChecks("/healthz");
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c=>
{
    c.SwaggerEndpoint("v1/swagger.json", "My API V1");
    c.EnableTryItOutByDefault();
});

app.UseHttpLogging();
app.UseHttpsRedirection();

app.UseJwtAuthorization();

app.UseHangfire();

app.MapControllers();
app.ApplyPostgresMigrations();

if (Program.IsNotAppTest())
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<IBonusProgramsRunner>().Init();
}

AddPostgresSeed(app.Services);



app.Run();


public partial class Program
{
    public static void AddPostgresSeed(IServiceProvider serviceProvider)
    {
        // Времянка пока юонусные программы захардкоженны
        using var scope1 = serviceProvider.CreateScope();
        {
            var postgres = scope1.ServiceProvider.GetRequiredService<PostgresDbContext>();
            var bp = postgres.BonusPrograms.FirstOrDefault(x => x.Id == new BonusProgramRep().Get().Id);
            if (bp == null)
            {
                bp = new BonusProgramRep().Get();
                bp.Id = default;
                postgres.BonusPrograms.Add(bp);
                postgres.SaveChanges();
            }
        }
    }
    public const string AppTest = nameof(AppTest);

    public static bool IsAllowDisableAuth() => IsNswagBuild() || IsAppTest();
    public static bool IsAppTest() => string.IsNullOrEmpty(Environment.GetEnvironmentVariable(AppTest)) == false;
    public static bool IsNotAppTest() => !IsAppTest();
    public static bool IsLocal() => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Local";
    public static bool IsNotLocal() => !IsLocal();
    public static bool IsNswagBuild() => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NswagGen")) == false;
    public static bool IsNotNswagBuild() => !IsNswagBuild();
}
