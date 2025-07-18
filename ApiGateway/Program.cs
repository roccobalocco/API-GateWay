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
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

builder.Services.AddHttpClient("RoomClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(10, 20))
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BackoffType = DelayBackoffType.Exponential;
    options.Retry.MaxDelay = TimeSpan.FromSeconds(2);
    options.CircuitBreaker.FailureRatio = 0.5;
    options.CircuitBreaker.MinimumThroughput = 10;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(20);
});

builder.Services.AddHttpClient("BookClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(8, 15))
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 4;
    options.Retry.BackoffType = DelayBackoffType.Exponential;
    options.Retry.MaxDelay = TimeSpan.FromSeconds(2);
    options.CircuitBreaker.FailureRatio = 0.4;
    options.CircuitBreaker.MinimumThroughput = 12;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(25);
});

builder.Services.AddHttpClient("LoanClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(6, 10))
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BackoffType = DelayBackoffType.Exponential;
    options.Retry.MaxDelay = TimeSpan.FromSeconds(3);
    options.CircuitBreaker.FailureRatio = 0.6;
    options.CircuitBreaker.MinimumThroughput = 8;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(40);
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("UserClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(Policy.BulkheadAsync<HttpResponseMessage>(10, 20))
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 5;
    options.Retry.BackoffType = DelayBackoffType.Exponential;
    options.Retry.MaxDelay = TimeSpan.FromSeconds(1);
    options.CircuitBreaker.FailureRatio = 0.75;
    options.CircuitBreaker.MinimumThroughput = 10;
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
    options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
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
        opt.QueueLimit = 50;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.AddPolicy("PerIpLimiter", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(ip, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 4,
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
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers()
    .RequireRateLimiting("FixedPolicy")
    .RequireRateLimiting("PerIpLimiter");

app.Run();
