namespace Virtusa.Agentic.OrchestratorApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;
using Virtusa.Agentic.OrchestratorApi.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class RunsController : ControllerBase
{
    private readonly IOrchestrationService _orchestrationService;
    private readonly IEventStore _eventStore;
    private readonly ILogger<RunsController> _logger;

    public RunsController(
        IOrchestrationService orchestrationService,
        IEventStore eventStore,
        ILogger<RunsController> logger)
    {
        _orchestrationService = orchestrationService;
        _eventStore = eventStore;
        _logger = logger;
    }

    /// <summary>
    /// Create a new SDLC automation run
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "EngineeringLeadOrAdmin")]
    public async Task<IActionResult> CreateRun([FromBody] OrchestratorRunRequest request)
    {
        if (request == null)
            return BadRequest("Request cannot be null");

        var runId = Guid.NewGuid().ToString();
        var correlationId = Guid.NewGuid().ToString();

        try
        {
            var executionPlan = await _orchestrationService.CreateExecutionPlan(request, runId);
            
            var response = new
            {
                runId,
                correlationId,
                status = AgentConstants.STATUS_QUEUED,
                executionPlan
            };

            _logger.LogInformation("Run created: {RunId}", runId);
            return CreatedAtAction(nameof(GetRunStatus), new { id = runId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create run");
            return StatusCode(500, new { error = "Failed to create run", details = ex.Message });
        }
    }

    /// <summary>
    /// Get status of a specific run
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRunStatus(string id)
    {
        try
        {
            var runStatus = await _orchestrationService.GetRunStatus(id);
            return Ok(runStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve run status for {RunId}", id);
            return StatusCode(500, new { error = "Failed to retrieve run status" });
        }
    }

    /// <summary>
    /// List all runs with pagination
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "EngineeringLeadOrAdmin")]
    public async Task<IActionResult> ListRuns([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        try
        {
            var runCreatedEvents = await _eventStore.QueryEventsByTypeAsync(nameof(RunCreatedEvent));
            var runIds = runCreatedEvents
                .Select(e => e.RunId)
                .Distinct()
                .Skip(skip)
                .Take(take)
                .ToList();

            var runs = new List<RunStatus>();
            foreach (var runId in runIds)
            {
                runs.Add(await _orchestrationService.GetRunStatus(runId));
            }

            return Ok(new { runs, total = runCreatedEvents.Select(e => e.RunId).Distinct().Count(), skip, take });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list runs");
            return StatusCode(500, new { error = "Failed to list runs" });
        }
    }

    /// <summary>
    /// Start a queued run
    /// </summary>
    [HttpPost("{id}/start")]
    [Authorize(Policy = "EngineeringLeadOrAdmin")]
    public async Task<IActionResult> StartRun(string id)
    {
        try
        {
            var result = await _orchestrationService.StartRun(id);
            if (result)
                return Ok(new { runId = id, status = AgentConstants.STATUS_RUNNING });
            
            return BadRequest(new { error = "Failed to start run" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start run {RunId}", id);
            return StatusCode(500, new { error = "Failed to start run" });
        }
    }

    /// <summary>
    /// Execute Phase 1 flow (requirements -> specification -> story)
    /// </summary>
    [HttpPost("{id}/execute-phase1")]
    [Authorize(Policy = "EngineeringLeadOrAdmin")]
    public async Task<IActionResult> ExecutePhase1Async(string id)
    {
        try
        {
            var status = await _orchestrationService.ExecutePhase1Async(id, HttpContext.RequestAborted);
            return Ok(new { runId = id, status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute phase 1 for run {RunId}", id);
            return StatusCode(500, new { error = "Failed to execute phase 1" });
        }
    }

    /// <summary>
    /// Execute Phase 2 flow (architecture -> task planning)
    /// </summary>
    [HttpPost("{id}/execute-phase2")]
    [Authorize(Policy = "EngineeringLeadOrAdmin")]
    public async Task<IActionResult> ExecutePhase2Async(string id)
    {
        try
        {
            var status = await _orchestrationService.ExecutePhase2Async(id, HttpContext.RequestAborted);
            return Ok(new { runId = id, status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute phase 2 for run {RunId}", id);
            return StatusCode(500, new { error = "Failed to execute phase 2" });
        }
    }

    /// <summary>
    /// Execute Phase 3 flow (test design -> code generation)
    /// </summary>
    [HttpPost("{id}/execute-phase3")]
    [Authorize(Policy = "EngineeringLeadOrAdmin")]
    public async Task<IActionResult> ExecutePhase3Async(string id)
    {
        try
        {
            var status = await _orchestrationService.ExecutePhase3Async(id, HttpContext.RequestAborted);
            return Ok(new { runId = id, status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute phase 3 for run {RunId}", id);
            return StatusCode(500, new { error = "Failed to execute phase 3" });
        }
    }

    /// <summary>
    /// Get artifacts for a run
    /// </summary>
    [HttpGet("{id}/artifacts")]
    [Authorize(Policy = "EngineeringLeadOrAdmin")]
    public async Task<IActionResult> GetArtifacts(string id)
    {
        try
        {
            var events = await _eventStore.QueryEventsAsync(id);
            var stageArtifacts = events
                .OfType<StageCompletedEvent>()
                .Select(e => new RunArtifactResponse
                {
                    ArtifactType = "stage_status",
                    Stage = e.StageName,
                    AgentName = e.AgentName,
                    Status = e.Status,
                    OccurredAt = e.OccurredAt,
                    Content = null
                });

            var agentArtifacts = events
                .OfType<AgentTaskCompletedEvent>()
                .Select(e => new RunArtifactResponse
                {
                    ArtifactType = "agent_output",
                    Stage = ResolveStageFromAgent(e.AgentName),
                    AgentName = e.AgentName,
                    Status = "completed",
                    OccurredAt = e.OccurredAt,
                    Content = ParseJsonOutput(e.AgentName, e.Output)
                });

            var artifacts = stageArtifacts
                .Concat(agentArtifacts)
                .OrderBy(a => a.OccurredAt)
                .ToList();

            return Ok(new { runId = id, artifacts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve artifacts for run {RunId}", id);
            return StatusCode(500, new { error = "Failed to retrieve artifacts" });
        }
    }

    private static string ResolveStageFromAgent(string agentName)
    {
        return agentName switch
        {
            AgentConstants.REQUIREMENTS => AgentConstants.STAGE_REQUIREMENTS,
            AgentConstants.SPECIFICATION => AgentConstants.STAGE_SPECIFICATION,
            AgentConstants.STORY => AgentConstants.STAGE_STORY,
            AgentConstants.ARCHITECTURE => AgentConstants.STAGE_ARCHITECTURE,
            AgentConstants.TASK_PLANNING => AgentConstants.STAGE_TASK_PLANNING,
            AgentConstants.TEST_DESIGN => AgentConstants.STAGE_TEST_DESIGN,
            AgentConstants.CODE_GENERATION => AgentConstants.STAGE_CODE_GENERATION,
            AgentConstants.IAC_CICD => AgentConstants.STAGE_IAC_CICD,
            AgentConstants.QUALITY_GATE => AgentConstants.STAGE_QUALITY_GATE,
            _ => agentName
        };
    }

    private static object? ParseJsonOutput(string agentName, string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        return agentName switch
        {
            AgentConstants.REQUIREMENTS => JsonSerializer.Deserialize<RequirementsAgentOutput>(output),
            AgentConstants.SPECIFICATION => JsonSerializer.Deserialize<SpecificationAgentOutput>(output),
            AgentConstants.STORY => JsonSerializer.Deserialize<StoryAgentOutput>(output),
            AgentConstants.ARCHITECTURE => JsonSerializer.Deserialize<ArchitectureAgentOutput>(output),
            AgentConstants.TASK_PLANNING => JsonSerializer.Deserialize<TaskPlanningAgentOutput>(output),
            AgentConstants.TEST_DESIGN => JsonSerializer.Deserialize<TestDesignAgentOutput>(output),
            AgentConstants.CODE_GENERATION => JsonSerializer.Deserialize<CodeGenerationAgentOutput>(output),
            AgentConstants.IAC_CICD => JsonSerializer.Deserialize<IaCCicdAgentOutput>(output),
            AgentConstants.QUALITY_GATE => JsonSerializer.Deserialize<QualityGateOutput>(output),
            _ => JsonSerializer.Deserialize<JsonElement>(output)
        };
    }

    private class RunArtifactResponse
    {
        public string ArtifactType { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public object? Content { get; set; }
    }
}
