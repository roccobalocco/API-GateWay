using EF.EF;
using EF.Models;
using LoanMS.Repository;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Utility.Interface;
using NLog.Targets;
using NLog.Config;
using Microsoft.EntityFrameworkCore;

var config = new LoggingConfiguration();

var ftarget = new FileTarget("file")
{
    FileName = "/app/output.log",  // path assoluto, assicurati esista e permessi
    Layout = "${longdate} ${level} ${message} ${exception}"
};

config.AddTarget(ftarget);
config.AddRuleForAllLevels(ftarget);

NLog.LogManager.Configuration = config;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddEntityFrameworkSqlServer()
    .AddSqlServer<GatewayContext>(builder.Configuration.GetConnectionString("Local"));

builder.Services.AddScoped<IGenericRepo<Loan>, LoanRepo>();

builder.Services.AddHttpClient("LoanClient");

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health/liveness", new HealthCheckOptions
{
    Predicate = _ => false // solo verifica che l'app sia attiva
});

app.MapHealthChecks("/health/readiness", new HealthCheckOptions
{
    Predicate = _ => true // esegue tutti gli health check registrati
});

// ➕ Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GatewayContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();