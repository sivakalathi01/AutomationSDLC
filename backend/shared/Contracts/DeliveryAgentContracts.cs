namespace Virtusa.Agentic.Shared.Contracts;

public class ArchitectureAgentOutput
{
    public List<ArchitectureDecisionRecord> Adrs { get; set; } = new();
    public string ComponentDesign { get; set; } = string.Empty;
    public List<string> Tradeoffs { get; set; } = new();
    public List<string> Risks { get; set; } = new();
}

public class ArchitectureDecisionRecord
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string Rationale { get; set; } = string.Empty;
}

public class TaskPlanningAgentOutput
{
    public List<TaskPlanItem> Tasks { get; set; } = new();
    public List<TaskDependency> DependencyGraph { get; set; } = new();
    public string SprintPlan { get; set; } = string.Empty;
}

public class TaskPlanItem
{
    public string Id { get; set; } = string.Empty;
    public string StoryId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Estimate { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
}

public class TaskDependency
{
    public string TaskId { get; set; } = string.Empty;
    public List<string> DependsOn { get; set; } = new();
}

public class TestDesignAgentOutput
{
    public string TestPlan { get; set; } = string.Empty;
    public List<TestCaseDefinition> TestCases { get; set; } = new();
    public CoverageTargets CoverageTargets { get; set; } = new();
    public QualityThresholds QualityThresholds { get; set; } = new();
}

public class TestCaseDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Linked { get; set; } = new();
}

public class CoverageTargets
{
    public int Unit { get; set; }
    public int Integration { get; set; }
}

public class QualityThresholds
{
    public int PassingTests { get; set; }
    public int MaxDefects { get; set; }
    public int Coverage { get; set; }
}

public class IaCCicdAgentOutput
{
    public List<IaCArtifact> IacArtifacts { get; set; } = new();
    public List<PipelineArtifact> PipelineArtifacts { get; set; } = new();
    public IaCValidationResults ValidationResults { get; set; } = new();
    public string DeploymentRunbook { get; set; } = string.Empty;
}

public class IaCArtifact
{
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class PipelineArtifact
{
    public string Path { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
}

public class IaCValidationResults
{
    public string IacValidation { get; set; } = string.Empty;
    public string PipelineLint { get; set; } = string.Empty;
}