using System.Diagnostics;
using System.Text.Json;
using StackExchange.Redis;

namespace DAYOrchestrator;

public class ServerManager
{
    private readonly Dictionary<string, ServerNode> _nodes = new();

    public ServerManager(IConnectionMultiplexer redis)
    {
        SetupSubscribers(redis);
        StartNewServer();
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
            
            // Server resources check. Currently not implemented on the server side.
            // if (response.CpuLoad > 70 || response.MemoryLoad > 70)
            // {
            //     StartNewServer();
            // }
            
            // Server player capacity check. (fires when server is more than 70% full)
            if (response.OnlinePlayers / (double) response.MaxPlayers > 0.7)
            {
                StartNewServer();
            }

            var node = _nodes[response.Identifier];
            node.LastResponse = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            node.OnlinePlayers = response.OnlinePlayers;
            node.MaxPlayers = response.MaxPlayers;
            _nodes[node.Identifier] = node;
        });
    }
    
    /// <summary>
    /// Example implementation for scaling servers on the local system. Can also be implemented using Docker swarm
    /// or Azure containers.
    /// </summary>
    private async void StartNewServer()
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "bash",
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };
        
        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST");
        var serverHost = Environment.GetEnvironmentVariable("PUBLIC_IP");
        var serverId = "days-stickworld-eu-" + _nodes.Count + 1;
        var serverPort = 15000 + _nodes.Count + 1;
        
        var command = $"docker run -d -p {serverPort}:7777/udp -e REDIS_HOST={redisHost} -e SERVER_CLUSTER=EU-1 -e SERVER_HOST={serverHost} -e SERVER_ID={serverId} -e SERVER_PORT={serverPort} ghcr.io/days-stickworld/game:latest";
        Console.WriteLine(command);
        
        process.Start();
        await process.StandardInput.WriteLineAsync("docker pull ghcr.io/days-stickworld/game:latest");
        await process.StandardInput.WriteLineAsync(command);
        process.Close();
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