using System.Data;
using System.Text.Json.Serialization;
using BonusService.Common;
using BonusService.Postgres;
using Correlate.DependencyInjection;
using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using MongoDB.Driver.Linq;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var services = builder.Services;

services.AddCorrelate(options => options.RequestHeaders = new []{ "X-Correlation-ID" });

builder.Logging.ClearProviders();
builder.Host.UseNLog();

services.TryAddTransient<IDateTimeService, DateTimeService>();
services.AddPostgres(configuration);
services.AddMongoService(configuration);
services.AddHangfireService(configuration);

services.AddFluentValidationAutoValidation();
services.AddFluentValidationClientsideAdapters();
services.AddValidatorsFromAssemblyContaining<Program>();
services.AddFluentValidationRulesToSwagger();

services.AddControllers().AddJsonOptions(opt=>
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

if (IsNswagBuild())
{
    //services.AddOpenApiDocument();
}


services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
services.AddSwaggerGen(c=> c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" }));

services.AddMediator(opt =>
{
    opt.ServiceLifetime = ServiceLifetime.Scoped;
});


var app = builder.Build();

if (IsNswagBuild())
{
    //app.UseOpenApi();
    //app.UseSwaggerUi3();
}


app.UseSwagger();
app.UseSwaggerUI(c=> c.SwaggerEndpoint("v1/swagger.json", "My API V1"));

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.ApplyPostgresMigrations();



var s = app.Services.CreateScope();
var _postgres = s.ServiceProvider.GetRequiredService<PostgresDbContext>();


var transactionDb = await _postgres.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
await _postgres.Transactions.BulkInsertAsync(new []{ new Transaction()
{
    BonusSum = 1,
    Description = "33",
    TransactionId = "34422423423423",
    LastUpdated = DateTimeOffset.UtcNow,
    BonusBase = 11,
    Type = TransactionType.Auto,
    BankId = 1,
    PersonId = Guid.NewGuid()
}});




app.Run();
record a(Guid PersonId, int BankId);
public partial class Program
{
    public static bool IsNswagBuild() => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NswagGen")) == false;
}
