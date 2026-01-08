using Microsoft.EntityFrameworkCore;
using Order.Api.Data;
using Order.Api.Data.Repository;
using Order.Api.Data.UnitOfWork;
using Order.Api.Messaging;
using Order.Api.Messaging.Events;
using Order.Api.Models;
using Order.Api.Outbox;
using Order.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add services to the container.
// Enables the API Explorer service for metadata generation
builder.Services.AddEndpointsApiExplorer();
// Adds Swagger generation services
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();

// Register Messaging
builder.Services.AddSingleton<IMqPublisher<object>, MqPublisher<object>>();
builder.Services.AddScoped<IOrderEvents, Order.Api.Messaging.Events.OrderEvents>();

// Register Services
builder.Services.AddScoped<IOrderService, OrderService>();

// Register Outbox
builder.Services.AddScoped<OutboxProcessor>();
builder.Services.AddHostedService<OutboxBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger(); // Serves the generated OpenAPI document
    app.UseSwaggerUI(); // Serves the Swagger UI
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
