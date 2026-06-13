namespace Virtusa.Agentic.Shared.Infrastructure;

using Virtusa.Agentic.Shared.Contracts;

/// <summary>
/// Extension methods for common validation and transformation logic.
/// </summary>
public static class ContractExtensions
{
    public static bool IsValidAgentName(string agentName)
    {
        return agentName switch
        {
            AgentConstants.ORCHESTRATOR => true,
            AgentConstants.REQUIREMENTS => true,
            AgentConstants.SPECIFICATION => true,
            AgentConstants.STORY => true,
            AgentConstants.ARCHITECTURE => true,
            AgentConstants.TASK_PLANNING => true,
            AgentConstants.TEST_DESIGN => true,
            AgentConstants.CODE_GENERATION => true,
            AgentConstants.IAC_CICD => true,
            AgentConstants.QUALITY_GATE => true,
            _ => false
        };
    }

    public static bool IsValidStatus(string status)
    {
        return status switch
        {
            AgentConstants.STATUS_QUEUED => true,
            AgentConstants.STATUS_RUNNING => true,
            AgentConstants.STATUS_BLOCKED => true,
            AgentConstants.STATUS_APPROVED => true,
            AgentConstants.STATUS_COMPLETED => true,
            AgentConstants.STATUS_FAILED => true,
            _ => false
        };
    }

    public static EnvelopeValidationResult ValidateEnvelope<T>(this AgentEnvelope<T> envelope)
    {
        var result = new EnvelopeValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(envelope.AgentName))
            result.Errors.Add("AgentName is required");
        else if (!IsValidAgentName(envelope.AgentName))
            result.Errors.Add($"Invalid AgentName: {envelope.AgentName}");

        if (string.IsNullOrWhiteSpace(envelope.RunId))
            result.Errors.Add("RunId is required");

        if (string.IsNullOrWhiteSpace(envelope.CorrelationId))
            result.Errors.Add("CorrelationId is required");

        if (envelope.Payload == null)
            result.Errors.Add("Payload cannot be null");

        if (result.Errors.Count > 0)
            result.IsValid = false;

        return result;
    }
}
