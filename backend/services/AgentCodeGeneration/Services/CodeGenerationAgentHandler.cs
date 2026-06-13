namespace Virtusa.Agentic.Agents.CodeGeneration.Services;
using Virtusa.Agentic.Shared.Contracts;

public class CodeGenerationAgentHandler
{
    private readonly ILogger<CodeGenerationAgentHandler> _logger;

    public CodeGenerationAgentHandler(ILogger<CodeGenerationAgentHandler> logger)
    {
        _logger = logger;
    }

    public Task<AgentEnvelope<CodeGenerationAgentOutput>> ExecuteAsync(AgentEnvelope<object> input)
    {
        _logger.LogInformation("Code generation agent processing run {RunId}", input.RunId);
        
        var output = new CodeGenerationAgentOutput
        {
            Changeset = new()
            {
                new Changeset { Path = "src/Controllers/AuthController.cs", ChangeType = "added" },
                new Changeset { Path = "src/Services/SsoService.cs", ChangeType = "added" }
            },
            BuildResult = new BuildResult { Status = "passed", Errors = new List<string>() },
            UnitTestResult = new TestResult { Status = "passed", PassRate = 95.5, TotalTests = 20, PassedTests = 19, FailedTests = 1 },
            StaticAnalysisSummary = new StaticAnalysisSummary { Critical = 0, High = 0, Medium = 2, Low = 5 }
        };

        return Task.FromResult(new AgentEnvelope<CodeGenerationAgentOutput>
        {
            AgentName = input.AgentName,
            RunId = input.RunId,
            CorrelationId = input.CorrelationId,
            Payload = output
        });
    }
}
