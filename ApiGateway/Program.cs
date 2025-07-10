using System.Text;
using ApiGateway.Options;
using EF.EF;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Polly;
using NLog.Targets;
using NLog.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var config = new LoggingConfiguration();
var ftarget = new FileTarget();
ftarget.FileName = "${basedir}/output.log";
config.AddTarget("file", ftarget);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// options
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

// Add fault tolerance policies
builder.Services.AddHttpClient("GatewayClient")
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.MinimumThroughput = 4;
        options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
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

var app = builder.Build();

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

app.UseAuthentication();  
app.UseAuthorization();

app.MapControllers();

app.Run();