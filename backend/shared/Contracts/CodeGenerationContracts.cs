namespace Virtusa.Agentic.Shared.Contracts;

// Code Generation Agent Input
public class CodeGenerationAgentInput
{
    public List<object> Tasks { get; set; } = new();
    public List<object> Adrs { get; set; } = new();
    public object TestPlan { get; set; } = new();
    public List<string> CodingStandards { get; set; } = new();
    public string TargetFramework { get; set; } = "dotnet8";
}

// Code Generation Agent Output
public class CodeGenerationAgentOutput
{
    public List<Changeset> Changeset { get; set; } = new();
    public BuildResult BuildResult { get; set; } = new();
    public TestResult UnitTestResult { get; set; } = new();
    public StaticAnalysisSummary StaticAnalysisSummary { get; set; } = new();
}

public class Changeset
{
    public string Path { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty; // added, modified, deleted
    public string? DiffContent { get; set; }
}

public class BuildResult
{
    public string Status { get; set; } = string.Empty; // passed, failed
    public string? LogRef { get; set; }
    public List<string>? Errors { get; set; }
}

public class TestResult
{
    public string Status { get; set; } = string.Empty; // passed, failed
    public double PassRate { get; set; }
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
}

public class StaticAnalysisSummary
{
    public int Critical { get; set; }
    public int High { get; set; }
    public int Medium { get; set; }
    public int Low { get; set; }
}
