namespace Virtusa.Agentic.Agents.TaskPlanning.Services;
using Virtusa.Agentic.Shared.Contracts;

public class TaskPlanningAgentHandler
{
    private readonly ILogger<TaskPlanningAgentHandler> _logger;

    public TaskPlanningAgentHandler(ILogger<TaskPlanningAgentHandler> logger)
    {
        _logger = logger;
    }

    public Task<AgentEnvelope<TaskPlanningAgentOutput>> ExecuteAsync(AgentEnvelope<object> input)
    {
        _logger.LogInformation("Task planning agent processing run {RunId}", input.RunId);
        
        var output = new TaskPlanningAgentOutput
        {
            Tasks = new()
            {
                new TaskPlanItem { Id = "TASK-001", StoryId = "STORY-001", Title = "Implement SSO controller", Estimate = "5d", Owner = "backend" },
                new TaskPlanItem { Id = "TASK-002", StoryId = "STORY-001", Title = "Write SSO integration tests", Estimate = "2d", Owner = "qa" }
            },
            DependencyGraph = new()
            {
                new TaskDependency { TaskId = "TASK-001", DependsOn = new() },
                new TaskDependency { TaskId = "TASK-002", DependsOn = new() { "TASK-001" } }
            },
            SprintPlan = "Sprint 1: 3 stories, 8 days capacity"
        };

        return Task.FromResult(new AgentEnvelope<TaskPlanningAgentOutput>
        {
            AgentName = input.AgentName,
            RunId = input.RunId,
            CorrelationId = input.CorrelationId,
            Payload = output
        });
    }
}
