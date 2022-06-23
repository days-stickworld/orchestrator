using Microsoft.AspNetCore.Mvc;

namespace DAYOrchestrator.Controllers;

[ApiController]
[Route("/api/endpoints")]
public class EndpointController : ControllerBase
{
    private readonly ServerManager _serverManager;
    
    public EndpointController(ServerManager serverManager)
    {
        _serverManager = serverManager;
    }

    [HttpGet]
    public ServerNode[] Get()
    {
        return _serverManager.GetActiveNodes();
    }
}