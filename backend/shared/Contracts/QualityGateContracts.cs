namespace Virtusa.Agentic.Shared.Contracts;

// Quality Gate Input
public class QualityGateInput
{
    public object BuildResult { get; set; } = new();
    public object TestResults { get; set; } = new();
    public object SecurityScanResults { get; set; } = new();
    public object PolicyResults { get; set; } = new();
    public object IaCResults { get; set; } = new();
}

// Quality Gate Output
public class QualityGateOutput
{
    public string Decision { get; set; } = string.Empty; // pass, fail, conditional_pass
    public List<string> Reasons { get; set; } = new();
    public List<RemediationAction> RemediationActions { get; set; } = new();
    public RiskSummary RiskSummary { get; set; } = new();
}

public class RemediationAction
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // critical, high, medium, low
    public string OwnerType { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class RiskSummary
{
    public string OverallRisk { get; set; } = string.Empty; // low, medium, high, critical
    public string Notes { get; set; } = string.Empty;
    public List<RiskItem>? RiskItems { get; set; }
}

public class RiskItem
{
    public string Category { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
