namespace ShareGateDemo.Shared;

public enum JobStatus
{
    Pending,
    Running,
    Completed,
    Failed
}

public sealed record MigrationJobDto(
    string Id,
    string Name,
    string Source,
    string Target,
    JobStatus Status,
    DateTime UpdatedAtUtc,
    string? Note);

public sealed record CreateJobRequest(
    string Name,
    string Source,
    string Target,
    string? Note);

public sealed record RunJobResponse(
    string Id,
    JobStatus Status);
