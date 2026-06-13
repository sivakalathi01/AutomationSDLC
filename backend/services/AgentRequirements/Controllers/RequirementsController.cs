namespace Virtusa.Agentic.Agents.Requirements.Controllers;

using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Agents.Requirements.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RequirementsController : ControllerBase
{
    private readonly IRequirementsAgentHandler _handler;
    private readonly ILogger<RequirementsController> _logger;

    public RequirementsController(
        IRequirementsAgentHandler handler,
        ILogger<RequirementsController> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeRequirementsAsync([FromBody] AgentEnvelope<RequirementsAgentInput> request)
    {
        try
        {
            var result = await _handler.ExecuteAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze requirements");
            return StatusCode(500, new { error = "Analysis failed", details = ex.Message });
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", agent = "requirements" });
    }
}
