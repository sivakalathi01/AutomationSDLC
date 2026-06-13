namespace Virtusa.Agentic.OrchestratorApi.Services;

using System.Net.Http.Json;
using Virtusa.Agentic.Shared.Contracts;
using Virtusa.Agentic.Shared.Infrastructure;

public interface IAgentHttpClient
{
    Uri? GetEndpoint(string agentName);
    Task<RequirementsAgentOutput> AnalyzeRequirementsAsync(string runId, string correlationId, RequirementsAgentInput input, CancellationToken cancellationToken = default);
    Task<SpecificationAgentOutput> GenerateSpecificationAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default);
    Task<StoryAgentOutput> GenerateStoryAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default);
    Task<ArchitectureAgentOutput> GenerateArchitectureAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default);
    Task<TaskPlanningAgentOutput> GenerateTaskPlanningAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default);
    Task<TestDesignAgentOutput> GenerateTestDesignAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default);
    Task<CodeGenerationAgentOutput> GenerateCodeGenerationAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default);
}

public class AgentHttpClient : IAgentHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly IAgentServiceRegistry _registry;

    public AgentHttpClient(HttpClient httpClient, IAgentServiceRegistry registry)
    {
        _httpClient = httpClient;
        _registry = registry;
    }

    public Uri? GetEndpoint(string agentName)
    {
        return _registry.GetAgentUri(agentName);
    }

    public async Task<RequirementsAgentOutput> AnalyzeRequirementsAsync(string runId, string correlationId, RequirementsAgentInput input, CancellationToken cancellationToken = default)
    {
        var endpoint = GetRequiredEndpoint(AgentConstants.REQUIREMENTS, "api/requirements/analyze");
        var request = new AgentEnvelope<RequirementsAgentInput>
        {
            AgentName = AgentConstants.REQUIREMENTS,
            Stage = AgentConstants.STAGE_REQUIREMENTS,
            RunId = runId,
            CorrelationId = correlationId,
            Payload = input
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<AgentEnvelope<RequirementsAgentOutput>>(cancellationToken: cancellationToken);
        return envelope?.Payload ?? throw new InvalidOperationException("Requirements agent returned an empty payload.");
    }

    public async Task<SpecificationAgentOutput> GenerateSpecificationAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default)
    {
        var endpoint = GetRequiredEndpoint(AgentConstants.SPECIFICATION, "api/specification/generate");
        var response = await _httpClient.PostAsJsonAsync(endpoint, input, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<AgentEnvelope<SpecificationAgentOutput>>(cancellationToken: cancellationToken);
        return envelope?.Payload ?? throw new InvalidOperationException("Specification agent returned an empty payload.");
    }

    public async Task<StoryAgentOutput> GenerateStoryAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default)
    {
        var endpoint = GetRequiredEndpoint(AgentConstants.STORY, "api/story/generate");
        var response = await _httpClient.PostAsJsonAsync(endpoint, input, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<AgentEnvelope<StoryAgentOutput>>(cancellationToken: cancellationToken);
        return envelope?.Payload ?? throw new InvalidOperationException("Story agent returned an empty payload.");
    }

    public async Task<ArchitectureAgentOutput> GenerateArchitectureAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default)
    {
        var endpoint = GetRequiredEndpoint(AgentConstants.ARCHITECTURE, "api/architecture/generate");
        var response = await _httpClient.PostAsJsonAsync(endpoint, input, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<AgentEnvelope<ArchitectureAgentOutput>>(cancellationToken: cancellationToken);
        return envelope?.Payload ?? throw new InvalidOperationException("Architecture agent returned an empty payload.");
    }

    public async Task<TaskPlanningAgentOutput> GenerateTaskPlanningAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default)
    {
        var endpoint = GetRequiredEndpoint(AgentConstants.TASK_PLANNING, "api/task-planning/generate");
        var response = await _httpClient.PostAsJsonAsync(endpoint, input, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<AgentEnvelope<TaskPlanningAgentOutput>>(cancellationToken: cancellationToken);
        return envelope?.Payload ?? throw new InvalidOperationException("Task planning agent returned an empty payload.");
    }

    public async Task<TestDesignAgentOutput> GenerateTestDesignAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default)
    {
        var endpoint = GetRequiredEndpoint(AgentConstants.TEST_DESIGN, "api/test-design/generate");
        var response = await _httpClient.PostAsJsonAsync(endpoint, input, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<AgentEnvelope<TestDesignAgentOutput>>(cancellationToken: cancellationToken);
        return envelope?.Payload ?? throw new InvalidOperationException("Test design agent returned an empty payload.");
    }

    public async Task<CodeGenerationAgentOutput> GenerateCodeGenerationAsync(string runId, string correlationId, object input, CancellationToken cancellationToken = default)
    {
        var endpoint = GetRequiredEndpoint(AgentConstants.CODE_GENERATION, "api/code-generation/generate");
        var response = await _httpClient.PostAsJsonAsync(endpoint, input, cancellationToken);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<AgentEnvelope<CodeGenerationAgentOutput>>(cancellationToken: cancellationToken);
        return envelope?.Payload ?? throw new InvalidOperationException("Code generation agent returned an empty payload.");
    }

    private Uri GetRequiredEndpoint(string agentName, string relativePath)
    {
        var baseUri = _registry.GetAgentUri(agentName)
            ?? throw new InvalidOperationException($"No endpoint configured for agent '{agentName}'.");

        return new Uri(baseUri, relativePath);
    }
}