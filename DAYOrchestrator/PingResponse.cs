namespace DAYOrchestrator;

public class PingResponse
{
    public string Identifier { get; set; } = "";
    public string Status { get; set; } = "";
    public int OnlinePlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int CpuLoad { get; set; }
    public int MemoryLoad { get; set; }
}