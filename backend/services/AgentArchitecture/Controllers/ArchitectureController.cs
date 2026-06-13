namespace Virtusa.Agentic.Agents.Architecture.Controllers;

using Microsoft.AspNetCore.Mvc;
using Virtusa.Agentic.Agents.Architecture.Services;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;

[ApiController]
[Route("api/architecture")]
public class ArchitectureController : ControllerBase
{
    private readonly ArchitectureAgentHandler _handler;

    public ArchitectureController(ArchitectureAgentHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateAsync([FromBody] object input)
    {
        var request = new AgentEnvelope<object>
        {
            AgentName = AgentConstants.ARCHITECTURE,
            Stage = AgentConstants.STAGE_ARCHITECTURE,
            RunId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            Payload = input
        };

        var response = await _handler.ExecuteAsync(request);
        return Ok(response);
    }
}