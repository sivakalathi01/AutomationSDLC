namespace Virtusa.Agentic.Agents.Specification.Controllers;

using Microsoft.AspNetCore.Mvc;
using Virtusa.Agentic.Agents.Specification.Services;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;

[ApiController]
[Route("api/specification")]
public class SpecificationController : ControllerBase
{
    private readonly ISpecificationAgentHandler _handler;

    public SpecificationController(ISpecificationAgentHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateAsync([FromBody] object specificationInput)
    {
        var request = new AgentEnvelope<object>
        {
            AgentName = AgentConstants.SPECIFICATION,
            Stage = AgentConstants.STAGE_SPECIFICATION,
            RunId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            Payload = specificationInput
        };

        var response = await _handler.ExecuteAsync(request);
        return Ok(response);
    }
}