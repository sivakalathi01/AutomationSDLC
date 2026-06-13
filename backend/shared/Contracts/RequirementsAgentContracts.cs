namespace Virtusa.Agentic.Shared.Contracts;

// Requirements Agent Input
public class RequirementsAgentInput
{
    public string BusinessProblem { get; set; } = string.Empty;
    public List<string> Stakeholders { get; set; } = new();
    public List<string> Constraints { get; set; } = new();
    public List<string>? DomainGlossary { get; set; }
}

// Requirements Agent Output
public class RequirementsAgentOutput
{
    public List<string> FunctionalRequirements { get; set; } = new();
    public NonFunctionalRequirements NonFunctionalRequirements { get; set; } = new();
    public List<string> Assumptions { get; set; } = new();
    public List<string> Ambiguities { get; set; } = new();
    public List<TraceLink> Traceability { get; set; } = new();
    public double ConfidenceScore { get; set; }
}

public class NonFunctionalRequirements
{
    public List<string> Security { get; set; } = new();
    public List<string> Performance { get; set; } = new();
    public List<string> Reliability { get; set; } = new();
    public List<string> Compliance { get; set; } = new();
}

public class TraceLink
{
    public string RequirementId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}
