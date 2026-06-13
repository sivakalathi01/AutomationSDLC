namespace Virtusa.Agentic.Agents.QualityGate.Controllers;

using Microsoft.AspNetCore.Mvc;
using Virtusa.Agentic.Agents.QualityGate.Services;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;

[ApiController]
[Route("api/quality-gate")]
public class QualityGateController : ControllerBase
{
    private readonly QualityGateAgentHandler _handler;

    public QualityGateController(QualityGateAgentHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("evaluate")]
    public async Task<IActionResult> EvaluateAsync([FromBody] AgentEnvelope<QualityGateInput> request)
    {
        request.AgentName = string.IsNullOrWhiteSpace(request.AgentName) ? AgentConstants.QUALITY_GATE : request.AgentName;
        request.Stage = string.IsNullOrWhiteSpace(request.Stage) ? AgentConstants.STAGE_QUALITY_GATE : request.Stage;
        request.RunId = string.IsNullOrWhiteSpace(request.RunId) ? Guid.NewGuid().ToString() : request.RunId;
        request.CorrelationId = string.IsNullOrWhiteSpace(request.CorrelationId) ? Guid.NewGuid().ToString() : request.CorrelationId;

        var response = await _handler.ExecuteAsync(request);
        return Ok(response);
    }
}