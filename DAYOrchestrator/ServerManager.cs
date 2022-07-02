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
            var response = JsonSerializer.Deserialize<PingResponse>(msg.ToString());
            if (response?.Status != "OK") _nodes.Remove(response!.Identifier);
            if (response.CpuLoad > 70 || response.MemoryLoad > 70 || (response.MaxPlayers - response.OnlinePlayers) < 30)
            {
                //TODO: Spin up new server
            }

            var node = _nodes[response.Identifier];
            node.LastResponse = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            node.OnlinePlayers = response.OnlinePlayers;
            node.MaxPlayers = response.MaxPlayers;
            _nodes.Add(node.Identifier, node);
        });
    }

    public ServerNode[] GetActiveNodes()
    {
        return _nodes.Values.ToArray();
    }

    public void RegisterUnresponsiveNode(string identifier)
    {
        _nodes.Remove(identifier);
    }
}