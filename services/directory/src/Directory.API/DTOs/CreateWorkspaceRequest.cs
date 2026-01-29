namespace Directory.API.DTOs;

public sealed record CreateWorkspaceRequest(
    string Name,
    string Slug);
