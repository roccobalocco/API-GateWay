using BookMS.Repository;
using EF.EF;
using EF.Models;
using Utility.Interface;
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

// Add repo
builder.Services.AddScoped<IGenericRepo<Book>, BookRepo>();

// Add fault tolerance policies
builder.Services.AddHttpClient("BookClient");

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