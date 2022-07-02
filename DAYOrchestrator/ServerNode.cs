namespace DAYOrchestrator;

public class ServerNode
{
    public string Identifier { get; set; } = "";
    public string Host { get; set; } = "";
    public string Type { get; set; } = "";
    public string Cluster { get; set; } = "";
    public int Port { get; set; }
    public int OnlinePlayers { get; set; }
    public int MaxPlayers { get; set; } = 1000;
    public long LastResponse { get; set; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();
}