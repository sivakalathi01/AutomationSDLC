namespace Virtusa.Agentic.Shared.Contracts;

public class SpecificationAgentOutput
{
    public string FunctionalSpec { get; set; } = string.Empty;
    public List<ApiContractDefinition> ApiContracts { get; set; } = new();
    public List<string> DomainModel { get; set; } = new();
    public List<string> EdgeCases { get; set; } = new();
    public List<TraceabilityLink> Traceability { get; set; } = new();
}

public class ApiContractDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

public class TraceabilityLink
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

public class StoryAgentOutput
{
    public List<string> Epics { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public List<StoryDefinition> Stories { get; set; } = new();
    public List<AcceptanceCriteriaDefinition> AcceptanceCriteria { get; set; } = new();
}

public class StoryDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public class AcceptanceCriteriaDefinition
{
    public string StoryId { get; set; } = string.Empty;
    public List<string> Criteria { get; set; } = new();
}