using System.Security.Claims;
using Authentication.API.Models;
using Authentication.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IKeycloakService _keycloakService;
    private readonly IDirectoryService _directoryService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IKeycloakService keycloakService, IDirectoryService directoryService, ILogger<AuthController> logger)
    {
        _keycloakService = keycloakService;
        _directoryService = directoryService;
        _logger = logger;
    }

    /// <summary>
    /// Exchange authorization code for access and refresh tokens
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> GetToken([FromBody] TokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tokenResponse = await _keycloakService.ExchangeCodeForTokenAsync(request.Code, request.RedirectUri, cancellationToken);
            return Ok(tokenResponse);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Invalid authorization code provided");
            return BadRequest(new { error = "invalid_grant", error_description = "Invalid authorization code" });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogError("Invalid client credentials");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "server_error", error_description = "Authentication configuration error" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token exchange");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "server_error", error_description = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Refresh an expired access token using a refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tokenResponse = await _keycloakService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
            return Ok(tokenResponse);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Invalid refresh token provided");
            return BadRequest(new { error = "invalid_grant", error_description = "Invalid or expired refresh token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "server_error", error_description = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Get information about the currently authenticated user from JWT claims
    /// </summary>
    [HttpGet("userinfo")]
    [Authorize]
    public ActionResult<UserInfoResponse> GetUserInfo()
    {
        try
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            var emailVerified = User.FindFirstValue("email_verified");
            var preferredUsername = User.FindFirstValue("preferred_username") ?? User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(preferredUsername))
            {
                _logger.LogWarning("Missing required claims in JWT token");
                return Unauthorized(new { error = "invalid_token", error_description = "Token is missing required claims" });
            }

            var userInfo = new UserInfoResponse
            {
                Sub = sub,
                Email = email,
                EmailVerified = bool.TryParse(emailVerified, out var verified) && verified,
                PreferredUsername = preferredUsername
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting user info from token claims");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "server_error", error_description = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Logout and revoke the refresh token
    /// </summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _keycloakService.RevokeTokenAsync(request.RefreshToken, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            // Return success anyway since logout is best-effort
            return NoContent();
        }
    }

    /// <summary>
    /// Authenticate with email and password (ROPC)
    /// </summary>
    [HttpPost("signin")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> SignIn([FromBody] SigninRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var tokenResponse = await _keycloakService.AuthenticateWithPasswordAsync(request.Email, request.Password, cancellationToken);
            return Ok(tokenResponse);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Invalid credentials for {Email}", request.Email);
            return Unauthorized(new { error = "invalid_credentials", error_description = "Invalid email or password" });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                                               (ex.StatusCode == System.Net.HttpStatusCode.BadRequest && ex.Message.Contains("Account is not fully set up")))
        {
            _logger.LogWarning("Account locked or rate limited for {Email}", request.Email);
            return StatusCode(StatusCodes.Status429TooManyRequests, new { error = "too_many_attempts", error_description = "Too many login attempts. Please try again later." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during sign in for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "server_error", error_description = "An unexpected error occurred" });
        }
    }

    /// <summary>
    /// Register a new user with organization
    /// </summary>
    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<ActionResult<SignupResponse>> SignUp([FromBody] SignupRequest request, CancellationToken cancellationToken)
    {
        string? createdUserId = null;
        try
        {
            // 1. Create user in Keycloak
            createdUserId = await _keycloakService.CreateUserAsync(request.Email, request.Password, request.FirstName, request.LastName, cancellationToken);
            _logger.LogInformation("Created Keycloak user {UserId} for {Email}", createdUserId, request.Email);

            // 2. Authenticate to get tokens
            var initialTokens = await _keycloakService.AuthenticateWithPasswordAsync(request.Email, request.Password, cancellationToken);

            // 3. Create organization via Directory service
            var orgSlug = GenerateSlug(request.OrganizationName);
            var organization = await _directoryService.CreateOrganizationAsync(request.OrganizationName, orgSlug, initialTokens.AccessToken, cancellationToken);
            _logger.LogInformation("Created organization {OrgId} ({OrgSlug}) for user {UserId}", organization.Id, orgSlug, createdUserId);

            // 4. Create default workspace
            var workspace = await _directoryService.CreateWorkspaceAsync(organization.Id, "General", "general", initialTokens.AccessToken, cancellationToken);
            _logger.LogInformation("Created workspace {WorkspaceId} for organization {OrgId}", workspace.Id, organization.Id);

            // 5. Add user as Owner member
            await _directoryService.AddMemberAsync(workspace.Id, createdUserId, "Owner", initialTokens.AccessToken, cancellationToken);

            // 6. Update Keycloak user attributes with org/tenant IDs
            var attributes = new Dictionary<string, List<string>>
            {
                ["organizationId"] = new List<string> { organization.Id },
                ["tenantId"] = new List<string> { organization.Id }
            };
            await _keycloakService.UpdateUserAttributesAsync(createdUserId, attributes, cancellationToken);

            // 7. Re-authenticate to get fresh token with org claims
            var finalTokens = await _keycloakService.AuthenticateWithPasswordAsync(request.Email, request.Password, cancellationToken);

            return Ok(new SignupResponse(
                finalTokens.AccessToken,
                finalTokens.RefreshToken,
                finalTokens.ExpiresIn,
                finalTokens.TokenType,
                createdUserId,
                organization.Id,
                workspace.Id));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("email already exists"))
        {
            _logger.LogWarning("Signup failed: email already exists for {Email}", request.Email);
            return Conflict(new { error = "email_exists", error_description = "A user with this email already exists" });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogWarning("Signup failed: conflict during resource creation for {Email}", request.Email);
            return Conflict(new { error = "resource_conflict", error_description = "Organization slug already exists. Please choose a different name." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during signup for {Email}. CreatedUserId: {UserId}", request.Email, createdUserId);

            // Attempt cleanup of Keycloak user on partial failure
            if (createdUserId != null)
            {
                try
                {
                    _logger.LogWarning("Attempting cleanup of partially created user {UserId}", createdUserId);
                    // Note: Cleanup would require a DeleteUserAsync method - for now we log the orphaned user
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Failed to cleanup user {UserId} after signup failure", createdUserId);
                }
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "server_error", error_description = "An unexpected error occurred during signup" });
        }
    }

    private static string GenerateSlug(string input)
    {
        var slug = input.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove special characters except hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Remove consecutive hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-{2,}", "-");

        // Trim hyphens from start/end
        slug = slug.Trim('-');

        // Limit length
        if (slug.Length > 50)
            slug = slug[..50].TrimEnd('-');

        return slug;
    }
}
