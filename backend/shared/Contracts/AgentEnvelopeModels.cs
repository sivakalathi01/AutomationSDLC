namespace Virtusa.Agentic.Shared.Contracts;

/// <summary>
/// Base envelope for all agent communications.
/// All agent inputs/outputs must conform to this envelope.
/// </summary>
public class AgentEnvelope<T>
{
    public string AgentName { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public T Payload { get; set; } = default!;
    public AgentMetadata Meta { get; set; } = new();
}

public class AgentMetadata
{
    public string CreatedBy { get; set; } = "system";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string UpdatedBy { get; set; } = "system";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Validation result for envelope compliance.
/// </summary>
public class EnvelopeValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
