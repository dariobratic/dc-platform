using System.Security.Claims;
using Authentication.API.Models;
using Authentication.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IKeycloakService keycloakService, ILogger<AuthController> logger)
    {
        _keycloakService = keycloakService;
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
}
