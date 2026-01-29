namespace AccessControl.API.DTOs;

public sealed record UpdateRoleRequest(
    string Name,
    string? Description,
    List<string> Permissions);
