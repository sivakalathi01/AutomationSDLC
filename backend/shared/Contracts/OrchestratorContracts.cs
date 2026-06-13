namespace Virtusa.Agentic.Shared.Contracts;

// Orchestrator Input
public class OrchestratorRunRequest
{
    public string Objective { get; set; } = string.Empty;
    public ProjectContext ProjectContext { get; set; } = new();
    public GovernanceProfile GovernanceProfile { get; set; } = new();
    public List<string> RequestedStages { get; set; } = new();
    public List<string> PriorArtifacts { get; set; } = new();
}

public class ProjectContext
{
    public string Domain { get; set; } = string.Empty;
    public string TargetStack { get; set; } = "dotnet_azure";
    public string Environment { get; set; } = "dev";
}

public class GovernanceProfile
{
    public string ApprovalMode { get; set; } = "hybrid"; // manual, hybrid, auto
    public string ComplianceLevel { get; set; } = "medium"; // low, medium, high, regulated
}

// Orchestrator Output
public class ExecutionPlan
{
    public List<ExecutionStep> Steps { get; set; } = new();
    public List<RoutingRule> RoutingTable { get; set; } = new();
    public List<CheckpointGate> CheckpointPlan { get; set; } = new();
    public string Status { get; set; } = "planned"; // planned, running, blocked, completed, failed
}

public class ExecutionStep
{
    public string Step { get; set; } = string.Empty;
    public string Agent { get; set; } = string.Empty;
    public List<string> DependsOn { get; set; } = new();
}

public class RoutingRule
{
    public string FromStage { get; set; } = string.Empty;
    public string ToAgent { get; set; } = string.Empty;
    public List<string> ArtifactTypes { get; set; } = new();
}

public class CheckpointGate
{
    public string Checkpoint { get; set; } = string.Empty;
    public bool ApprovalRequired { get; set; }
    public List<string> Roles { get; set; } = new();
}

// Run Status
public class RunStatus
{
    public string RunId { get; set; } = string.Empty;
    public string Status { get; set; } = "queued"; // queued, running, blocked, approved, completed, failed
    public List<StageStatus> Stages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class StageStatus
{
    public string StageName { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
