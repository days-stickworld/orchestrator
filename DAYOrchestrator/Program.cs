using DAYOrchestrator;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHostedService<PingService>();

var redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_HOST")!);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

var manager = new ServerManager(redis);
builder.Services.AddSingleton(manager);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();