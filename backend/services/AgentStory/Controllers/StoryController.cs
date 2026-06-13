namespace Virtusa.Agentic.Agents.Story.Controllers;

using Microsoft.AspNetCore.Mvc;
using Virtusa.Agentic.Agents.Story.Services;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;

[ApiController]
[Route("api/story")]
public class StoryController : ControllerBase
{
    private readonly IStoryAgentHandler _handler;

    public StoryController(IStoryAgentHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateAsync([FromBody] object storyInput)
    {
        var request = new AgentEnvelope<object>
        {
            AgentName = AgentConstants.STORY,
            Stage = AgentConstants.STAGE_STORY,
            RunId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            Payload = storyInput
        };

        var response = await _handler.ExecuteAsync(request);
        return Ok(response);
    }
}