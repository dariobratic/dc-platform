# Keycloak Integration - DC Platform

## Architecture: Single Realm with Tenant Isolation

DC Platform uses a **single Keycloak realm** (`dc-platform`) for all tenants. Tenant isolation is achieved through user attributes and request headers, not separate realms.

### Multi-Organization Support

- Users can belong to **multiple organizations** (via Directory service Memberships)
- JWT token contains `userId` only - NOT organizationId
- Active organization/workspace selected via **HTTP headers**
- Backend validates user has access to requested org/workspace

### Context Headers

```
X-Organization-Id: <guid>    # Required for org-scoped operations
X-Workspace-Id: <guid>       # Optional, for workspace-scoped operations
```

Frontend flow:
1. User logs in → receives token with userId
2. Frontend fetches user's organizations from Directory service
3. User selects organization (picker if multiple)
4. Frontend includes `X-Organization-Id` header on all requests
5. Backend validates membership before processing

---

## Keycloak Client Configuration

The Gateway service acts as the single OAuth2 client:

```
Client ID:       dc-platform-gateway
Client Type:     Confidential
Root URL:        http://localhost:5000
Valid Redirect:  http://localhost:3000/*, http://localhost:5173/*
Web Origins:     http://localhost:3000, http://localhost:5173
```

Backend services do NOT have their own Keycloak clients — they validate tokens issued to the Gateway client.

---

## .NET Integration Patterns

### JWT Validation in Program.cs

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Keycloak:RequireHttps");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "preferred_username"
        };
    });

builder.Services.AddAuthorization();
```

Middleware order in the pipeline:

```csharp
app.UseCors();
app.UseAuthentication();
app.UseOrganizationContext();  // Custom middleware - validates X-Organization-Id
app.UseAuthorization();
app.MapControllers();
```

### Organization Context Middleware

Validates that user has access to the requested organization:

```csharp
public class OrganizationContextMiddleware
{
    private readonly RequestDelegate _next;

    public OrganizationContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IMembershipRepository membershipRepo)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        var orgHeader = context.Request.Headers["X-Organization-Id"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(orgHeader))
        {
            // No org context - some endpoints don't require it (e.g., list my orgs)
            await _next(context);
            return;
        }

        if (!Guid.TryParse(orgHeader, out var organizationId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid X-Organization-Id header" });
            return;
        }

        var userId = Guid.Parse(context.User.FindFirstValue("sub")!);
        
        // Validate user has access to this organization
        var hasAccess = await membershipRepo.UserBelongsToOrganizationAsync(userId, organizationId);
        
        if (!hasAccess)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Access denied to organization" });
            return;
        }

        // Store in HttpContext for downstream use
        context.Items["OrganizationId"] = organizationId;
        
        // Handle workspace context if provided
        var wsHeader = context.Request.Headers["X-Workspace-Id"].FirstOrDefault();
        if (Guid.TryParse(wsHeader, out var workspaceId))
        {
            var wsAccess = await membershipRepo.UserBelongsToWorkspaceAsync(userId, workspaceId);
            if (!wsAccess)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new { error = "Access denied to workspace" });
                return;
            }
            context.Items["WorkspaceId"] = workspaceId;
        }

        await _next(context);
    }
}

public static class OrganizationContextMiddlewareExtensions
{
    public static IApplicationBuilder UseOrganizationContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<OrganizationContextMiddleware>();
    }
}
```

### Current User Service

Extracts user identity and context from token + headers:

```csharp
// Application layer interface
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    string Username { get; }
    bool IsAuthenticated { get; }
    
    // From headers (validated by middleware)
    Guid? OrganizationId { get; }
    Guid? WorkspaceId { get; }
    
    // Helper methods
    Guid RequireOrganizationId();
    Guid RequireWorkspaceId();
}

// Infrastructure implementation
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext Context => _httpContextAccessor.HttpContext 
        ?? throw new InvalidOperationException("No HTTP context");
    
    private ClaimsPrincipal? User => Context.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId =>
        Guid.Parse(User?.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Missing sub claim"));

    public string Email =>
        User?.FindFirstValue("email")
            ?? throw new UnauthorizedAccessException("Missing email claim");

    public string Username =>
        User?.FindFirstValue("preferred_username") ?? Email;

    // From validated middleware
    public Guid? OrganizationId => 
        Context.Items["OrganizationId"] as Guid?;

    public Guid? WorkspaceId => 
        Context.Items["WorkspaceId"] as Guid?;

    public Guid RequireOrganizationId() =>
        OrganizationId ?? throw new InvalidOperationException("Organization context required. Include X-Organization-Id header.");

    public Guid RequireWorkspaceId() =>
        WorkspaceId ?? throw new InvalidOperationException("Workspace context required. Include X-Workspace-Id header.");
}
```

Register in DI:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
```

### Using in Controllers

```csharp
[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class WorkspacesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public WorkspacesController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWorkspaces()
    {
        // Organization from validated header
        var orgId = _currentUser.RequireOrganizationId();
        var query = new GetWorkspacesByOrganizationQuery(orgId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
```

---

## Token Structure

### JWT Claims (from Keycloak)

| Claim | Source | Type | Description |
|-------|--------|------|-------------|
| `sub` | Keycloak standard | `Guid` | User ID (Keycloak user ID) |
| `email` | Keycloak standard | `string` | User email address |
| `preferred_username` | Keycloak standard | `string` | Display name |
| `email_verified` | Keycloak standard | `bool` | Email verification status |

### Context (from HTTP Headers)

| Header | Required | Description |
|--------|----------|-------------|
| `X-Organization-Id` | For org-scoped ops | Active organization GUID |
| `X-Workspace-Id` | For workspace-scoped ops | Active workspace GUID |

### Example Decoded JWT Payload

```json
{
  "sub": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "iss": "http://localhost:8080/realms/dc-platform",
  "aud": "dc-platform-gateway",
  "exp": 1706550000,
  "iat": 1706546400,
  "email": "user@example.com",
  "email_verified": true,
  "preferred_username": "john.doe"
}
```

Note: Organization and workspace context come from headers, NOT from token.

---

## Role Mapping

Keycloak provides **authentication** (who is the user).
Access Control service provides **authorization** (what can they do).

Basic Keycloak realm roles (optional, for coarse-grained access):

| Keycloak Role | Purpose |
|---------------|---------|
| `platform-admin` | Anthropic/DC staff - full platform access |
| `user` | Regular authenticated user |

Fine-grained permissions (workspace roles, resource permissions) are managed by:
- Directory service (workspace membership + role)
- Access Control service (RBAC/ABAC policies)

---

## Configuration Templates

### appsettings.json — Keycloak Connection

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/dc-platform",
    "Audience": "dc-platform-gateway",
    "RequireHttps": false
  }
}
```

### appsettings.Production.json — Production Overrides

```json
{
  "Keycloak": {
    "Authority": "https://auth.example.com/realms/dc-platform",
    "Audience": "dc-platform-gateway",
    "RequireHttps": true
  }
}
```

---

## NuGet Packages

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.0" />
```

---

## Frontend Integration

### Login Flow

```typescript
// 1. Redirect to Keycloak login
window.location.href = `${KEYCLOAK_URL}/protocol/openid-connect/auth?` +
  `client_id=dc-platform-gateway&` +
  `redirect_uri=${FRONTEND_URL}/callback&` +
  `response_type=code&` +
  `scope=openid email profile`;

// 2. Exchange code for token (via Gateway)
const token = await fetch('/api/auth/token', {
  method: 'POST',
  body: JSON.stringify({ code, redirect_uri })
});

// 3. Fetch user's organizations
const orgs = await fetch('/api/v1/users/me/organizations', {
  headers: { 'Authorization': `Bearer ${token}` }
});

// 4. User selects organization, store in app state
setActiveOrganization(orgs[0].id);

// 5. All subsequent requests include header
fetch('/api/v1/workspaces', {
  headers: {
    'Authorization': `Bearer ${token}`,
    'X-Organization-Id': activeOrganizationId
  }
});
```

### Organization Switcher Component

```typescript
// User can switch organization without re-login
function switchOrganization(newOrgId: string) {
  setActiveOrganization(newOrgId);
  // Refresh current view with new org context
  refetchData();
}
```

---

## Keycloak Realm Setup Checklist

1. Create realm: `dc-platform`
2. Create client: `dc-platform-gateway` (confidential)
3. Configure client redirect URIs and web origins
4. Create realm roles: `platform-admin`, `user`
5. Assign default role `user` to new users
6. Enable email verification (optional)
7. Configure password policies
8. Set token lifetimes (access: 5min, refresh: 30min recommended)
