namespace Virtusa.Agentic.Agents.Story.Services;
using Virtusa.Agentic.Shared.Contracts;

public interface IStoryAgentHandler
{
    Task<AgentEnvelope<StoryAgentOutput>> ExecuteAsync(AgentEnvelope<object> input);
}

public class StoryAgentHandler : IStoryAgentHandler
{
    private readonly ILogger<StoryAgentHandler> _logger;

    public StoryAgentHandler(ILogger<StoryAgentHandler> logger)
    {
        _logger = logger;
    }

    public Task<AgentEnvelope<StoryAgentOutput>> ExecuteAsync(AgentEnvelope<object> input)
    {
        _logger.LogInformation("Story agent processing run {RunId}", input.RunId);
        
        var output = new StoryAgentOutput
        {
            Epics = new() { "User Management", "Project Collaboration", "Reporting" },
            Features = new() { "SSO Integration", "Multi-tenant isolation", "Real-time collaboration" },
            Stories = new()
            {
                new StoryDefinition { Id = "STORY-001", Title = "As a user, I want to sign in via SSO", Priority = 1 },
                new StoryDefinition { Id = "STORY-002", Title = "As a team member, I want to collaborate in real-time", Priority = 2 }
            },
            AcceptanceCriteria = new()
            {
                new AcceptanceCriteriaDefinition { StoryId = "STORY-001", Criteria = new() { "SSO endpoint integrated", "Token validation works", "Session persists" } }
            }
        };

        return Task.FromResult(new AgentEnvelope<StoryAgentOutput>
        {
            AgentName = input.AgentName,
            RunId = input.RunId,
            CorrelationId = input.CorrelationId,
            Payload = output
        });
    }
}
