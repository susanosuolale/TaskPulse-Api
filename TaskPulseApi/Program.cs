using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using TaskPulse.Application.DTOs;
using TaskPulse.Application.Interfaces;
using TaskPulse.Infrastructure.Data;
using TaskPulse.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add Redis Caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    // prefix for your cached data
    options.InstanceName = "TaskPulse_";
});

// Add services to the container.
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<TaskResponseDto>("Tasks");

builder.Services.AddControllers().AddOData(
    options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100).AddRouteComponents(
        "odata",
        modelBuilder.GetEdmModel()));

// Configure Entity Framework Core with SQL Server
// Every app needs a specific address to find its database. This address is called a "Connection String".
// this address is stored in appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
