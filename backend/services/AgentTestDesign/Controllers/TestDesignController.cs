namespace Virtusa.Agentic.Agents.TestDesign.Controllers;

using Microsoft.AspNetCore.Mvc;
using Virtusa.Agentic.Agents.TestDesign.Services;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;

[ApiController]
[Route("api/test-design")]
public class TestDesignController : ControllerBase
{
    private readonly TestDesignAgentHandler _handler;

    public TestDesignController(TestDesignAgentHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateAsync([FromBody] object input)
    {
        var request = new AgentEnvelope<object>
        {
            AgentName = AgentConstants.TEST_DESIGN,
            Stage = AgentConstants.STAGE_TEST_DESIGN,
            RunId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            Payload = input
        };

        var response = await _handler.ExecuteAsync(request);
        return Ok(response);
    }
}