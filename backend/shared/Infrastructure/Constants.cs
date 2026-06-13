namespace Virtusa.Agentic.Shared.Infrastructure;

public static class AgentConstants
{
    // Agent Names
    public const string ORCHESTRATOR = "orchestrator";
    public const string REQUIREMENTS = "requirements";
    public const string SPECIFICATION = "specification";
    public const string STORY = "story";
    public const string ARCHITECTURE = "architecture";
    public const string TASK_PLANNING = "task_planning";
    public const string TEST_DESIGN = "test_design";
    public const string CODE_GENERATION = "code_generation";
    public const string IAC_CICD = "iac_cicd";
    public const string QUALITY_GATE = "quality_security_gate";

    // Stages
    public const string STAGE_REQUIREMENTS = "requirements";
    public const string STAGE_SPECIFICATION = "specification";
    public const string STAGE_STORY = "story";
    public const string STAGE_ARCHITECTURE = "architecture";
    public const string STAGE_TASK_PLANNING = "task_planning";
    public const string STAGE_TEST_DESIGN = "test_design";
    public const string STAGE_CODE_GENERATION = "code_generation";
    public const string STAGE_IAC_CICD = "iac_cicd";
    public const string STAGE_QUALITY_GATE = "quality_gate";

    // Status Values
    public const string STATUS_QUEUED = "queued";
    public const string STATUS_PLANNED = "planned";
    public const string STATUS_RUNNING = "running";
    public const string STATUS_BLOCKED = "blocked";
    public const string STATUS_APPROVED = "approved";
    public const string STATUS_COMPLETED = "completed";
    public const string STATUS_FAILED = "failed";

    // Service Bus Topics
    public const string TOPIC_AGENT_TASKS = "agent-tasks";
    public const string TOPIC_QUALITY_GATES = "quality-gates";
    public const string TOPIC_APPROVALS = "approvals";
    public const string TOPIC_RUN_EVENTS = "run-events";
    public const string TOPIC_AGENT_RESULTS = "agent-results";
}

public static class StorageConstants
{
    // Cosmos DB Containers
    public const string CONTAINER_EVENTS = "events";
    public const string CONTAINER_RUNS = "runs";
    public const string CONTAINER_ARTIFACTS = "artifacts";
    public const string CONTAINER_APPROVALS = "approvals";

    // Blob Storage Paths
    public const string BLOB_REQUIREMENTS_PATH = "requirements/{0}";
    public const string BLOB_SPECS_PATH = "specifications/{0}";
    public const string BLOB_CODE_PATH = "code/{0}";
    public const string BLOB_LOGS_PATH = "logs/{0}";
}

public static class RbacRoles
{
    public const string PLATFORM_ADMIN = "PlatformAdmin";
    public const string ARCHITECT_REVIEWER = "ArchitectReviewer";
    public const string SECURITY_REVIEWER = "SecurityReviewer";
    public const string ENGINEERING_LEAD = "EngineeringLead";
    public const string DEVELOPER = "Developer";
    public const string READONLY_AUDITOR = "ReadOnlyAuditor";
}

public static class ApiRoutes
{
    // Orchestrator
    public const string RUNS_CREATE = "/api/runs";
    public const string RUNS_LIST = "/api/runs";
    public const string RUNS_DETAIL = "/api/runs/{id}";
    public const string RUNS_ARTIFACTS = "/api/runs/{id}/artifacts";

    // Approvals
    public const string APPROVALS_LIST = "/api/approvals";
    public const string APPROVALS_DECISION = "/api/approvals/{id}/decision";

    // Quality
    public const string QUALITY_RESULTS = "/api/quality/{runId}";

    // Audit
    public const string AUDIT_LOGS = "/api/audit";
}
