namespace Virtusa.Agentic.Shared.Contracts;

/// <summary>
/// Immutable event for the shared event store (Cosmos DB).
/// All state changes must be recorded as events.
/// </summary>
public abstract class DomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// Run Lifecycle Events
public class RunCreatedEvent : DomainEvent
{
    public string Objective { get; set; } = string.Empty;
    public ProjectContext ProjectContext { get; set; } = new();
}

public class RunStartedEvent : DomainEvent
{
    public List<string> PlanStages { get; set; } = new();
}

public class StageCompletedEvent : DomainEvent
{
    public string StageName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // success, blocked_approval, failed
}

public class RunCompletedEvent : DomainEvent
{
    public string FinalStatus { get; set; } = string.Empty; // success, failed, partial
    public List<string>? FailureReasons { get; set; }
}

// Agent Execution Events
public class AgentTaskAssignedEvent : DomainEvent
{
    public string TaskId { get; set; } = string.Empty;
    public string Input { get; set; } = string.Empty; // JSON
}

public class AgentTaskCompletedEvent : DomainEvent
{
    public string TaskId { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty; // JSON
    public double ExecutionTimeMs { get; set; }
}

public class AgentTaskFailedEvent : DomainEvent
{
    public string TaskId { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public bool Retryable { get; set; }
}

// Quality Gate Events
public class QualityGateEvaluatedEvent : DomainEvent
{
    public string Decision { get; set; } = string.Empty; // pass, fail, conditional_pass
    public List<string> FailureReasons { get; set; } = new();
    public List<GateViolation> Violations { get; set; } = new();
}

public class GateViolation
{
    public string Gate { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // critical, high, medium, low
    public string Description { get; set; } = string.Empty;
}

// Approval Events
public class ApprovalRequestedEvent : DomainEvent
{
    public string Stage { get; set; } = string.Empty;
    public string ApproverId { get; set; } = string.Empty;
    public string ArtifactId { get; set; } = string.Empty;
}

public class ApprovalProvidedEvent : DomainEvent
{
    public string Stage { get; set; } = string.Empty;
    public string ApproverId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty; // approved, rejected, request_changes
    public string? Reason { get; set; }
}

public class EventStoreQueryResponse
{
    public List<DomainEvent> Events { get; set; } = new();
    public long TotalCount { get; set; }
    public string? ContinuationToken { get; set; }
}
