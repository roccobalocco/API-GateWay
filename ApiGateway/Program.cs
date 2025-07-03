using ApiGateway.Options;
using EF.EF;
using Polly;
using NLog.Targets;
using NLog.Config;

var config = new LoggingConfiguration();
var ftarget = new FileTarget();
ftarget.FileName = "${basedir}/output.log";
config.AddTarget("file", ftarget);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEntityFrameworkSqlServer()
    .AddSqlServer<GatewayContext>(builder.Configuration.GetConnectionString("Local"));

// Add fault tolerance policies
builder.Services.AddHttpClient("GatewayClient")
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.CircuitBreaker.FailureRatio = 0.5; // 50% failure rate
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.MinimumThroughput = 4;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
        // puoi configurare anche timeout, hedging, ecc.
    });

// Add optionsbuilder.Services.Configure<MicroServicesOptions>(
builder.Configuration.GetSection(nameof(MicroServicesOptions));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();