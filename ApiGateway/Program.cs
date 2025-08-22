using System.Text;
using ApiGateway.Options;
using EF.EF;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Polly;
using NLog.Targets;
using NLog.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using ApiGateway.Services;
using Prometheus;

var config = new LoggingConfiguration();

var ftarget = new FileTarget("file")
{
    FileName = "/app/output.log", // path assoluto, assicurati esista e permessi
    Layout = "${longdate} ${level} ${message} ${exception}"
};

config.AddTarget(ftarget);
config.AddRuleForAllLevels(ftarget);

NLog.LogManager.Configuration = config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// OPTIONS
builder.Services.Configure<MicroServicesOptions>(
    builder.Configuration.GetSection(nameof(MicroServicesOptions)));
builder.Services.Configure<UsersAllowedOptions>(
    builder.Configuration.GetSection(nameof(UsersAllowedOptions)));
builder.Services.Configure<JWTOptions>(
    builder.Configuration.GetSection(nameof(JWTOptions)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insert the JWT in the following format: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT" 
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "Bearer",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            []
        }
    });
});

builder.Services.AddDbContext<GatewayContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Local")));

// ➕ FAULT TOLERANCE PER SERVICE USING NAMED HTTP CLIENTS

builder.Services.AddHttpClient("RoomClient", client => { client.Timeout = TimeSpan.FromSeconds(5); })
    .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(20, 50))
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 5;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.MaxDelay = TimeSpan.FromSeconds(2.5);
        options.CircuitBreaker.FailureRatio = 0.05;
        options.CircuitBreaker.MinimumThroughput = 75;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(35);
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(6);
    });

builder.Services.AddHttpClient("BookClient", client => { client.Timeout = TimeSpan.FromSeconds(5); })
    .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(20, 50))
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 5;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.MaxDelay = TimeSpan.FromSeconds(2.5);
        options.CircuitBreaker.FailureRatio = 0.05;
        options.CircuitBreaker.MinimumThroughput = 75;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(35);
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(6);
    });

builder.Services.AddHttpClient("LoanClient", client => { client.Timeout = TimeSpan.FromSeconds(5); })
    .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(20, 50))
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 5;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.MaxDelay = TimeSpan.FromSeconds(2.5);
        options.CircuitBreaker.FailureRatio = 0.05;
        options.CircuitBreaker.MinimumThroughput = 75;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(35);
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(6);
    });

builder.Services.AddHttpClient("UserClient", client => { client.Timeout = TimeSpan.FromSeconds(5); })
    .AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(20, 50))
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 5;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.MaxDelay = TimeSpan.FromSeconds(2.5);
        options.CircuitBreaker.FailureRatio = 0.05;
        options.CircuitBreaker.MinimumThroughput = 75;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(35);
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(6);
    });

// ➕ RATE LIMITING (.NET 8)
builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = (context, token) =>
    {
        var logger = context.HttpContext?.RequestServices
            .GetRequiredService<ILogger<Program>>();

        var ip = context.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.HttpContext?.Request.Path.Value ?? "unknown";

        logger?.LogWarning("Rate limit rejected request. IP: {IP}, Path: {Path}", ip, path);

        return ValueTask.CompletedTask;
    };

    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.PermitLimit = 800;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 225;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.AddPolicy("PerIpLimiter", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 400,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 100,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
    });
});


var jwtOptions = builder.Configuration.GetSection(nameof(JWTOptions)).Get<JWTOptions>();
var key = Encoding.ASCII.GetBytes(jwtOptions!.SecretKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton<MetricsService>();

var app = builder.Build();

app.UseHttpMetrics();
app.MapHealthChecks("/health/liveness", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/readiness", new HealthCheckOptions { Predicate = _ => true });

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GatewayContext>();
    db.Database.Migrate();
}

// For project purpose I find useful to have access at the swagger
// if (app.Environment.IsDevelopment())
// {
app.UseStaticFiles(); // Place before UseSwaggerUI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
    c.RoutePrefix = "swagger";
});

// }

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapMetrics();

app.MapControllers()
    .RequireRateLimiting("PerIpLimiter")
    .RequireRateLimiting("FixedPolicy");

app.Run();