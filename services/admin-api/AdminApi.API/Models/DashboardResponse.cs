namespace AdminApi.API.Models;

public record DashboardResponse(
    int OrganizationCount,
    int AuditEntryCount,
    DateTime GeneratedAt);
