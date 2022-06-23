using System.Text.Json;
using StackExchange.Redis;

namespace DAYOrchestrator;

public class ServerManager
{
    private readonly Dictionary<string, ServerNode> _nodes = new();

    public ServerManager(IConnectionMultiplexer redis)
    {
        SetupSubscribers(redis);
    }
    
    private void SetupSubscribers(IConnectionMultiplexer redis)
    {
        var sub = redis.GetSubscriber();
        sub.Subscribe("server:register", (_, msg) =>
        {
            try
            {
                var node = JsonSerializer.Deserialize<ServerNode>(msg.ToString());
                _nodes.Add(node!.Identifier, node);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });

        sub.Subscribe("server:status", (_, msg) =>
        {
            Console.WriteLine(msg.ToString());
            var response = JsonSerializer.Deserialize<PingResponse>(msg.ToString());
            if (response?.Status != "OK") _nodes.Remove(response!.Identifier);
            if (response.CpuLoad > 70 || response.MemoryLoad > 70 || (response.MaxPlayers - response.OnlinePlayers) < 30)
            {
                //TODO: Spin up new server
            }
        });
    }

    public ServerNode[] GetActiveNodes()
    {
        return _nodes.Values.ToArray();
    }
}