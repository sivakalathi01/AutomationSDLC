namespace Virtusa.Agentic.Agents.TaskPlanning.Controllers;

using Microsoft.AspNetCore.Mvc;
using Virtusa.Agentic.Agents.TaskPlanning.Services;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;

[ApiController]
[Route("api/task-planning")]
public class TaskPlanningController : ControllerBase
{
    private readonly TaskPlanningAgentHandler _handler;

    public TaskPlanningController(TaskPlanningAgentHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateAsync([FromBody] object input)
    {
        var request = new AgentEnvelope<object>
        {
            AgentName = AgentConstants.TASK_PLANNING,
            Stage = AgentConstants.STAGE_TASK_PLANNING,
            RunId = Guid.NewGuid().ToString(),
            CorrelationId = Guid.NewGuid().ToString(),
            Payload = input
        };

        var response = await _handler.ExecuteAsync(request);
        return Ok(response);
    }
}