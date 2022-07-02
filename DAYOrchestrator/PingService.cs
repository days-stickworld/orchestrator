using StackExchange.Redis;

namespace DAYOrchestrator;

public class PingService : IHostedService, IDisposable
{
    private readonly ISubscriber _sub;
    private readonly ServerManager _serverManager;
    private Timer? _timer;

    public PingService(IConnectionMultiplexer redis, ServerManager serverManager)
    {
        _sub = redis.GetSubscriber();
        _serverManager = serverManager;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        _sub.Publish("server:ping", "");

        var time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        foreach (var node in _serverManager.GetActiveNodes())
        {
            var diff = time - node.LastResponse;
            if (diff > 10000)
            {
                _serverManager.RegisterUnresponsiveNode(node.Identifier);
            }
        }
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