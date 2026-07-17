using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using TaskPulse.Application.DTOs;
using TaskPulse.Application.Interfaces;
using TaskPulse.Application.Services;
using TaskPulse.Infrastructure.Data;
using TaskPulse.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Use local memory cache instead of Redis so the app doesn't crash when Redis is turned off!
builder.Services.AddDistributedMemoryCache();

// Add services to the container.
// var modelBuilder = new ODataConventionModelBuilder();
// modelBuilder.EntitySet<TaskResponseDto>("Tasks");

builder.Services.AddControllersWithViews();
// .AddOData(
//    options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100).AddRouteComponents(
//        "odata",
//        modelBuilder.GetEdmModel()));

// Configure Entity Framework Core with SQL Server
// Dual Database Support:
// 1. Local Development uses your Windows SQL Server LocalDB.
// 2. Production (Render.com) uses a free In-Memory database!
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    else
    {
        options.UseInMemoryDatabase("TaskPulseOnlineDb");
    }
});

// Dependency Injection
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();

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

app.MapControllerRoute(
    name: "default",
    // says the dafault controller is tasks and default action is index
    pattern: "{controller=Tasks}/{action=Index}/{id?}");

app.Run();
