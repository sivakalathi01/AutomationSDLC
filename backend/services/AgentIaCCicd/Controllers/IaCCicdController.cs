namespace Virtusa.Agentic.Agents.IaCCicd.Controllers;

using Microsoft.AspNetCore.Mvc;
using Virtusa.Agentic.Agents.IaCCicd.Services;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;

[ApiController]
[Route("api/iac-cicd")]
public class IaCCicdController : ControllerBase
{
    private readonly IaCCicdAgentHandler _handler;

    public IaCCicdController(IaCCicdAgentHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateAsync([FromBody] object input)
    {
        var request = new AgentEnvelope<object>
        {
            AgentName = AgentConstants.IAC_CICD,
            Stage = AgentConstants.STAGE_IAC_CICD,
            RunId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            Payload = input
        };

        var response = await _handler.ExecuteAsync(request);
        return Ok(response);
    }
}