namespace Virtusa.Agentic.Shared.Contracts;

/// <summary>
/// Messages for Azure Service Bus async communication between services.
/// </summary>

// Commands (request -> response pattern)
public abstract class ServiceBusCommand
{
    public string CommandId { get; set; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
}

// Agent Task Command
public class ExecuteAgentTaskCommand : ServiceBusCommand
{
    public string RunId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string InputPayload { get; set; } = string.Empty; // JSON
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
}

// Quality Gate Command
public class ExecuteQualityGateCommand : ServiceBusCommand
{
    public string RunId { get; set; } = string.Empty;
    public string BuildArtifactPath { get; set; } = string.Empty;
    public string TestResultsPath { get; set; } = string.Empty;
    public string SecurityScanResultPath { get; set; } = string.Empty;
}

// Approval Command
public class RequestApprovalCommand : ServiceBusCommand
{
    public string RunId { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string ArtifactId { get; set; } = string.Empty;
    public List<string> ApprovalRoles { get; set; } = new();
    public int TimeoutMinutes { get; set; } = 60;
}

// Events (pub-sub pattern)
public abstract class ServiceBusEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}

public class AgentTaskCompletedMessage : ServiceBusEvent
{
    public string RunId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string OutputPayload { get; set; } = string.Empty; // JSON
    public double ExecutionTimeMs { get; set; }
}

public class AgentTaskFailedMessage : ServiceBusEvent
{
    public string RunId { get; set; } = string.Empty;
    public string TaskId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public bool Retryable { get; set; }
}

public class QualityGateResultMessage : ServiceBusEvent
{
    public string RunId { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty; // pass, fail, conditional_pass
    public List<string> FailureReasons { get; set; } = new();
}

public class ApprovalDecisionMessage : ServiceBusEvent
{
    public string RunId { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty; // approved, rejected, request_changes
    public string ApprovedBy { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class RunProgressMessage : ServiceBusEvent
{
    public string RunId { get; set; } = string.Empty;
    public string CurrentStage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
}
