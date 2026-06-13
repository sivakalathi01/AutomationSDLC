namespace Virtusa.Agentic.Agents.CodeGeneration.Controllers;

using Microsoft.AspNetCore.Mvc;
using Virtusa.Agentic.Agents.CodeGeneration.Services;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;

[ApiController]
[Route("api/code-generation")]
public class CodeGenerationController : ControllerBase
{
    private readonly CodeGenerationAgentHandler _handler;

    public CodeGenerationController(CodeGenerationAgentHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateAsync([FromBody] object input)
    {
        var request = new AgentEnvelope<object>
        {
            AgentName = AgentConstants.CODE_GENERATION,
            Stage = AgentConstants.STAGE_CODE_GENERATION,
            RunId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            Payload = input
        };

        var response = await _handler.ExecuteAsync(request);
        return Ok(response);
    }
}