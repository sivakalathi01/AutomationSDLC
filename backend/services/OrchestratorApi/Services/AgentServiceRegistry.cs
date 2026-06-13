namespace Virtusa.Agentic.OrchestratorApi.Services;

using Microsoft.Extensions.Options;

public interface IAgentServiceRegistry
{
    Uri? GetAgentUri(string agentName);
}

public class AgentServiceRegistry : IAgentServiceRegistry
{
    private readonly AgentServiceOptions _options;

    public AgentServiceRegistry(IOptions<AgentServiceOptions> options)
    {
        _options = options.Value;
    }

    public Uri? GetAgentUri(string agentName)
    {
        var value = agentName switch
        {
            "requirements" => _options.Requirements,
            "specification" => _options.Specification,
            "story" => _options.Story,
            "architecture" => _options.Architecture,
            "task_planning" => _options.TaskPlanning,
            "test_design" => _options.TestDesign,
            "code_generation" => _options.CodeGeneration,
            "iac_cicd" => _options.IaCCicd,
            "quality_security_gate" => _options.QualityGate,
            _ => string.Empty
        };

        return Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri : null;
    }
}