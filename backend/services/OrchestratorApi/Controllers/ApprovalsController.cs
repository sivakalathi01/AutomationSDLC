namespace Virtusa.Agentic.OrchestratorApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ApprovalsController : ControllerBase
{
    private readonly ILogger<ApprovalsController> _logger;

    public ApprovalsController(ILogger<ApprovalsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get pending approvals for current user
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "EngineeringLeadOrAdmin")]
    public IActionResult GetPendingApprovalsAsync()
    {
        try
        {
            // Placeholder: Query approvals from event store or database
            var approvals = new List<object>();
            return Ok(new { approvals, total = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve approvals");
            return StatusCode(500, new { error = "Failed to retrieve approvals" });
        }
    }

    /// <summary>
    /// Provide approval decision
    /// </summary>
    [HttpPost("{id}/decision")]
    [Authorize(Policy = "EngineeringLeadOrAdmin")]
    public IActionResult ProvideApprovalDecisionAsync(string id, [FromBody] ApprovalDecisionRequest request)
    {
        try
        {
            // Placeholder: Record approval decision
            return Ok(new { id, decision = request.Decision, approvedAt = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record approval decision");
            return StatusCode(500, new { error = "Failed to record approval decision" });
        }
    }

    public class ApprovalDecisionRequest
    {
        public string Decision { get; set; } = string.Empty; // approved, rejected, request_changes
        public string? Reason { get; set; }
    }
}
