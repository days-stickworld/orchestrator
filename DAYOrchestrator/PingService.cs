using StackExchange.Redis;

namespace DAYOrchestrator;

public class PingService : IHostedService, IDisposable
{
    private readonly ILogger<PingService> _logger;
    private readonly ISubscriber _sub;
    private Timer? _timer;

    public PingService(ILogger<PingService> logger, IConnectionMultiplexer redis)
    {
        _logger = logger;
        _sub = redis.GetSubscriber();
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _sub.Publish("server:ping", "");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}