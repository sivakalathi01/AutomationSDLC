namespace Virtusa.Agentic.OrchestratorApi.Services;

using System.Text.Json;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;

/// <summary>
/// Main orchestration service that coordinates agent execution and workflow.
/// </summary>
public interface IOrchestrationService
{
    Task<ExecutionPlan> CreateExecutionPlan(OrchestratorRunRequest request, string runId);
    Task<RunStatus> GetRunStatus(string runId);
    Task<bool> StartRun(string runId);
    Task<RunStatus> ExecutePhase1Async(string runId, CancellationToken cancellationToken = default);
    Task<RunStatus> ExecutePhase2Async(string runId, CancellationToken cancellationToken = default);
    Task<RunStatus> ExecutePhase3Async(string runId, CancellationToken cancellationToken = default);
    Task<bool> CompleteStage(string runId, string stageName);
    Task<bool> FailStage(string runId, string stageName, string error);
}

public class OrchestrationService : IOrchestrationService
{
    private readonly IEventStore _eventStore;
    private readonly IServiceBusPublisher _serviceBusPublisher;
    private readonly IAgentHttpClient _agentHttpClient;
    private readonly ILogger<OrchestrationService> _logger;

    public OrchestrationService(
        IEventStore eventStore,
        IServiceBusPublisher serviceBusPublisher,
        IAgentHttpClient agentHttpClient,
        ILogger<OrchestrationService> logger)
    {
        _eventStore = eventStore;
        _serviceBusPublisher = serviceBusPublisher;
        _agentHttpClient = agentHttpClient;
        _logger = logger;
    }

    public async Task<ExecutionPlan> CreateExecutionPlan(OrchestratorRunRequest request, string runId)
    {
        // Log run creation event
        var runCreatedEvent = new RunCreatedEvent
        {
            RunId = runId,
            Objective = request.Objective,
            ProjectContext = request.ProjectContext,
            CorrelationId = Guid.NewGuid().ToString()
        };

        await _eventStore.AppendEventAsync(runCreatedEvent);

        // Create execution plan based on requested stages
        var executionPlan = new ExecutionPlan
        {
            Status = AgentConstants.STATUS_PLANNED,
            Steps = GenerateExecutionSteps(request.RequestedStages),
            RoutingTable = GenerateRoutingTable(request.RequestedStages),
            CheckpointPlan = GenerateCheckpoints(request.GovernanceProfile)
        };

        foreach (var step in executionPlan.Steps)
        {
            var endpoint = _agentHttpClient.GetEndpoint(step.Agent);
            if (endpoint != null)
            {
                _logger.LogInformation("Resolved endpoint for agent {Agent}: {Endpoint}", step.Agent, endpoint);
            }
        }

        _logger.LogInformation("Execution plan created for run {RunId}", runId);
        return executionPlan;
    }

    public async Task<RunStatus> GetRunStatus(string runId)
    {
        var events = await _eventStore.QueryEventsAsync(runId);
        
        var runStatus = new RunStatus
        {
            RunId = runId,
            CreatedAt = events.FirstOrDefault()?.OccurredAt ?? DateTime.UtcNow,
            Status = DetermineOverallStatus(events),
            Stages = BuildStageStatuses(events)
        };

        return runStatus;
    }

    public async Task<bool> StartRun(string runId)
    {
        var runStartedEvent = new RunStartedEvent
        {
            RunId = runId,
            CorrelationId = Guid.NewGuid().ToString(),
            PlanStages = new List<string>
            {
                AgentConstants.STAGE_REQUIREMENTS,
                AgentConstants.STAGE_SPECIFICATION,
                AgentConstants.STAGE_STORY,
                AgentConstants.STAGE_ARCHITECTURE,
                AgentConstants.STAGE_CODE_GENERATION,
                AgentConstants.STAGE_QUALITY_GATE
            }
        };

        await _eventStore.AppendEventAsync(runStartedEvent);
        _logger.LogInformation("Run {RunId} started", runId);
        return true;
    }

    public async Task<RunStatus> ExecutePhase1Async(string runId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var runCreatedEvent = (await _eventStore.QueryEventsAsync(runId))
            .OfType<RunCreatedEvent>()
            .FirstOrDefault();

        if (runCreatedEvent == null)
        {
            throw new InvalidOperationException($"Run '{runId}' was not found.");
        }

        await StartRun(runId);

        var requirementsInput = new RequirementsAgentInput
        {
            BusinessProblem = runCreatedEvent.Objective,
            Stakeholders = new List<string> { "engineering", "platform", "security" },
            Constraints = new List<string>
            {
                $"Domain: {runCreatedEvent.ProjectContext.Domain}",
                $"Target stack: {runCreatedEvent.ProjectContext.TargetStack}",
                $"Environment: {runCreatedEvent.ProjectContext.Environment}"
            }
        };

        var requirementsOutput = await _agentHttpClient.AnalyzeRequirementsAsync(runId, correlationId, requirementsInput, cancellationToken);
        await RecordAgentOutputAsync(runId, correlationId, AgentConstants.REQUIREMENTS, requirementsOutput);
        await CompleteStage(runId, AgentConstants.STAGE_REQUIREMENTS);

        var specificationInput = new
        {
            functionalRequirements = requirementsOutput.FunctionalRequirements,
            nonFunctionalRequirements = requirementsOutput.NonFunctionalRequirements,
            targetArchitectureConstraints = requirementsInput.Constraints
        };

        var specificationOutput = await _agentHttpClient.GenerateSpecificationAsync(runId, correlationId, specificationInput, cancellationToken);
        await RecordAgentOutputAsync(runId, correlationId, AgentConstants.SPECIFICATION, specificationOutput);
        await CompleteStage(runId, AgentConstants.STAGE_SPECIFICATION);

        var storyInput = new
        {
            specification = specificationOutput,
            releaseScope = "phase1",
            capacityHints = new { sprintLengthDays = 10, teamSize = 4 }
        };

        var storyOutput = await _agentHttpClient.GenerateStoryAsync(runId, correlationId, storyInput, cancellationToken);
        await RecordAgentOutputAsync(runId, correlationId, AgentConstants.STORY, storyOutput);
        await CompleteStage(runId, AgentConstants.STAGE_STORY);

        return await GetRunStatus(runId);
    }

    public async Task<RunStatus> ExecutePhase2Async(string runId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var events = await _eventStore.QueryEventsAsync(runId);
        var runCreatedEvent = events.OfType<RunCreatedEvent>().FirstOrDefault();

        if (runCreatedEvent == null)
        {
            throw new InvalidOperationException($"Run '{runId}' was not found.");
        }

        var specificationOutput = events
            .OfType<AgentTaskCompletedEvent>()
            .Where(e => e.AgentName == AgentConstants.SPECIFICATION)
            .Select(e => JsonSerializer.Deserialize<SpecificationAgentOutput>(e.Output))
            .LastOrDefault();

        var storyOutput = events
            .OfType<AgentTaskCompletedEvent>()
            .Where(e => e.AgentName == AgentConstants.STORY)
            .Select(e => JsonSerializer.Deserialize<StoryAgentOutput>(e.Output))
            .LastOrDefault();

        if (specificationOutput == null || storyOutput == null)
        {
            await ExecutePhase1Async(runId, cancellationToken);
            events = await _eventStore.QueryEventsAsync(runId);
            specificationOutput = events
                .OfType<AgentTaskCompletedEvent>()
                .Where(e => e.AgentName == AgentConstants.SPECIFICATION)
                .Select(e => JsonSerializer.Deserialize<SpecificationAgentOutput>(e.Output))
                .LastOrDefault();
            storyOutput = events
                .OfType<AgentTaskCompletedEvent>()
                .Where(e => e.AgentName == AgentConstants.STORY)
                .Select(e => JsonSerializer.Deserialize<StoryAgentOutput>(e.Output))
                .LastOrDefault();
        }

        if (specificationOutput == null || storyOutput == null)
        {
            throw new InvalidOperationException("Phase 2 requires Phase 1 outputs, but they could not be resolved.");
        }

        var architectureInput = new
        {
            specification = specificationOutput,
            nfrs = new { reliability = "standard", security = "enterprise" },
            enterpriseStandards = new[] { "managed-identity", "key-vault", "container-apps" }
        };

        var architectureOutput = await _agentHttpClient.GenerateArchitectureAsync(runId, correlationId, architectureInput, cancellationToken);
        await RecordAgentOutputAsync(runId, correlationId, AgentConstants.ARCHITECTURE, architectureOutput);
        await CompleteStage(runId, AgentConstants.STAGE_ARCHITECTURE);

        var taskPlanningInput = new
        {
            stories = storyOutput.Stories,
            adrs = architectureOutput.Adrs,
            teamConventions = new[] { "dotnet", "xunit", "azure" }
        };

        var taskPlanningOutput = await _agentHttpClient.GenerateTaskPlanningAsync(runId, correlationId, taskPlanningInput, cancellationToken);
        await RecordAgentOutputAsync(runId, correlationId, AgentConstants.TASK_PLANNING, taskPlanningOutput);
        await CompleteStage(runId, AgentConstants.STAGE_TASK_PLANNING);

        return await GetRunStatus(runId);
    }

    public async Task<RunStatus> ExecutePhase3Async(string runId, CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();
        var events = await _eventStore.QueryEventsAsync(runId);

        var specificationOutput = events
            .OfType<AgentTaskCompletedEvent>()
            .Where(e => e.AgentName == AgentConstants.SPECIFICATION)
            .Select(e => JsonSerializer.Deserialize<SpecificationAgentOutput>(e.Output))
            .LastOrDefault();

        var taskPlanningOutput = events
            .OfType<AgentTaskCompletedEvent>()
            .Where(e => e.AgentName == AgentConstants.TASK_PLANNING)
            .Select(e => JsonSerializer.Deserialize<TaskPlanningAgentOutput>(e.Output))
            .LastOrDefault();

        if (specificationOutput == null || taskPlanningOutput == null)
        {
            await ExecutePhase2Async(runId, cancellationToken);
            events = await _eventStore.QueryEventsAsync(runId);
            specificationOutput = events
                .OfType<AgentTaskCompletedEvent>()
                .Where(e => e.AgentName == AgentConstants.SPECIFICATION)
                .Select(e => JsonSerializer.Deserialize<SpecificationAgentOutput>(e.Output))
                .LastOrDefault();
            taskPlanningOutput = events
                .OfType<AgentTaskCompletedEvent>()
                .Where(e => e.AgentName == AgentConstants.TASK_PLANNING)
                .Select(e => JsonSerializer.Deserialize<TaskPlanningAgentOutput>(e.Output))
                .LastOrDefault();
        }

        if (specificationOutput == null || taskPlanningOutput == null)
        {
            throw new InvalidOperationException("Phase 3 execution requires Phase 2 outputs, but they could not be resolved.");
        }

        var testDesignInput = new
        {
            requirements = specificationOutput.Traceability.Select(t => t.From).ToList(),
            acceptanceCriteria = new[] { "Story acceptance criteria available" },
            apiContracts = specificationOutput.ApiContracts
        };

        var testDesignOutput = await _agentHttpClient.GenerateTestDesignAsync(runId, correlationId, testDesignInput, cancellationToken);
        await RecordAgentOutputAsync(runId, correlationId, AgentConstants.TEST_DESIGN, testDesignOutput);
        await CompleteStage(runId, AgentConstants.STAGE_TEST_DESIGN);

        var codeGenerationInput = new
        {
            tasks = taskPlanningOutput.Tasks,
            adrs = new[] { "Architecture approved" },
            testPlan = testDesignOutput.TestPlan,
            codingStandards = new[] { "dotnet", "clean-code", "secure-coding" },
            targetFramework = "net7.0"
        };

        var codeGenerationOutput = await _agentHttpClient.GenerateCodeGenerationAsync(runId, correlationId, codeGenerationInput, cancellationToken);
        await RecordAgentOutputAsync(runId, correlationId, AgentConstants.CODE_GENERATION, codeGenerationOutput);
        await CompleteStage(runId, AgentConstants.STAGE_CODE_GENERATION);

        return await GetRunStatus(runId);
    }

    public async Task<bool> CompleteStage(string runId, string stageName)
    {
        var stageCompletedEvent = new StageCompletedEvent
        {
            RunId = runId,
            CorrelationId = Guid.NewGuid().ToString(),
            StageName = stageName,
            Status = "success"
        };

        await _eventStore.AppendEventAsync(stageCompletedEvent);
        _logger.LogInformation("Stage {StageName} completed for run {RunId}", stageName, runId);
        return true;
    }

    public async Task<bool> FailStage(string runId, string stageName, string error)
    {
        var stageCompletedEvent = new StageCompletedEvent
        {
            RunId = runId,
            CorrelationId = Guid.NewGuid().ToString(),
            StageName = stageName,
            Status = "failed"
        };

        await _eventStore.AppendEventAsync(stageCompletedEvent);
        _logger.LogError("Stage {StageName} failed for run {RunId}: {Error}", stageName, runId, error);
        return true;
    }

    private List<ExecutionStep> GenerateExecutionSteps(List<string> requestedStages)
    {
        var steps = new List<ExecutionStep>();
        
        if (requestedStages.Contains(AgentConstants.STAGE_REQUIREMENTS))
            steps.Add(new ExecutionStep { Step = "1", Agent = AgentConstants.REQUIREMENTS, DependsOn = new() });

        if (requestedStages.Contains(AgentConstants.STAGE_SPECIFICATION))
            steps.Add(new ExecutionStep { Step = "2", Agent = AgentConstants.SPECIFICATION, DependsOn = new() { "1" } });

        if (requestedStages.Contains(AgentConstants.STAGE_STORY))
            steps.Add(new ExecutionStep { Step = "3", Agent = AgentConstants.STORY, DependsOn = new() { "2" } });

        if (requestedStages.Contains(AgentConstants.STAGE_CODE_GENERATION))
            steps.Add(new ExecutionStep { Step = "4", Agent = AgentConstants.CODE_GENERATION, DependsOn = new() { "3" } });

        if (requestedStages.Contains(AgentConstants.STAGE_QUALITY_GATE))
            steps.Add(new ExecutionStep { Step = "5", Agent = AgentConstants.QUALITY_GATE, DependsOn = new() { "4" } });

        return steps;
    }

    private List<RoutingRule> GenerateRoutingTable(List<string> stages)
    {
        return new List<RoutingRule>
        {
            new RoutingRule { FromStage = AgentConstants.STAGE_REQUIREMENTS, ToAgent = AgentConstants.SPECIFICATION, ArtifactTypes = new() { "requirements" } },
            new RoutingRule { FromStage = AgentConstants.STAGE_SPECIFICATION, ToAgent = AgentConstants.STORY, ArtifactTypes = new() { "specification" } },
            new RoutingRule { FromStage = AgentConstants.STAGE_STORY, ToAgent = AgentConstants.CODE_GENERATION, ArtifactTypes = new() { "stories" } },
            new RoutingRule { FromStage = AgentConstants.STAGE_CODE_GENERATION, ToAgent = AgentConstants.QUALITY_GATE, ArtifactTypes = new() { "code" } }
        };
    }

    private List<CheckpointGate> GenerateCheckpoints(GovernanceProfile governance)
    {
        var checkpoints = new List<CheckpointGate>();

        if (governance.ApprovalMode == "manual" || governance.ApprovalMode == "hybrid")
        {
            checkpoints.Add(new CheckpointGate
            {
                Checkpoint = "requirements_approval",
                ApprovalRequired = true,
                Roles = new() { RbacRoles.ARCHITECT_REVIEWER, RbacRoles.ENGINEERING_LEAD }
            });

            checkpoints.Add(new CheckpointGate
            {
                Checkpoint = "code_review",
                ApprovalRequired = true,
                Roles = new() { RbacRoles.ARCHITECT_REVIEWER }
            });

            checkpoints.Add(new CheckpointGate
            {
                Checkpoint = "security_review",
                ApprovalRequired = governance.ComplianceLevel == "high" || governance.ComplianceLevel == "regulated",
                Roles = new() { RbacRoles.SECURITY_REVIEWER }
            });
        }

        return checkpoints;
    }

    private string DetermineOverallStatus(List<DomainEvent> events)
    {
        if (!events.Any()) return AgentConstants.STATUS_QUEUED;
        if (events.OfType<RunCompletedEvent>().Any()) return AgentConstants.STATUS_COMPLETED;
        if (events.OfType<RunCreatedEvent>().Any()) return AgentConstants.STATUS_RUNNING;
        return AgentConstants.STATUS_RUNNING;
    }

    private List<StageStatus> BuildStageStatuses(List<DomainEvent> events)
    {
        var stages = new List<StageStatus>();
        var stageEvents = events.OfType<StageCompletedEvent>();

        foreach (var stageEvent in stageEvents)
        {
            stages.Add(new StageStatus
            {
                StageName = stageEvent.StageName,
                Status = stageEvent.Status,
                CompletedAt = stageEvent.OccurredAt
            });
        }

        return stages;
    }

    private async Task RecordAgentOutputAsync(string runId, string correlationId, string agentName, object output)
    {
        var completedEvent = new AgentTaskCompletedEvent
        {
            RunId = runId,
            CorrelationId = correlationId,
            AgentName = agentName,
            TaskId = $"{agentName}-{Guid.NewGuid():N}",
            Output = JsonSerializer.Serialize(output),
            ExecutionTimeMs = 0,
            EventType = nameof(AgentTaskCompletedEvent)
        };

        await _eventStore.AppendEventAsync(completedEvent);
    }
}
