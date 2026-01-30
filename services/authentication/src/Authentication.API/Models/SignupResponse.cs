namespace Authentication.API.Models;

public record SignupResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType,
    string UserId,
    string OrganizationId,
    string WorkspaceId);
