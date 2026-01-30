---
name: keycloak-integration
description: |
  Keycloak OAuth2/OIDC code integration. Use when implementing JWT validation,
  authentication middleware, token handling, or organization context in .NET or Vue.js
  applications. For Keycloak server configuration, see keycloak-admin skill.
---

# Keycloak Integration - Code Patterns

> For Keycloak server configuration (realm-export.json, client scopes, troubleshooting), see `.claude/skills/keycloak-admin/SKILL.md`.

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

## Keycloak Clients

Two clients exist in the `dc-platform` realm:

| Client | Type | Purpose |
|--------|------|---------|
| `dc-platform-gateway` | Confidential | Backend services validate tokens against this audience |
| `dc-platform-admin` | Public (PKCE S256) | Frontend SPA uses this for login via oidc-client-ts |

Backend services do NOT have their own Keycloak clients — they validate tokens issued to the Gateway client. For full client configuration details, see the `keycloak-admin` skill.

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

## Frontend Integration (Vue.js Shell App)

The shell app (`apps/shell`) handles all auth using `oidc-client-ts` with the `dc-platform-admin` public client (Authorization Code + PKCE).

### UserManager Setup

```typescript
// apps/shell/src/plugins/auth.ts
import { UserManager, WebStorageStateStore } from 'oidc-client-ts'

export const userManager = new UserManager({
  authority: `${import.meta.env.VITE_KEYCLOAK_URL}/realms/${import.meta.env.VITE_KEYCLOAK_REALM}`,
  client_id: import.meta.env.VITE_KEYCLOAK_CLIENT_ID, // dc-platform-admin
  redirect_uri: `${window.location.origin}/callback`,
  post_logout_redirect_uri: window.location.origin,
  response_type: 'code',
  scope: 'openid profile email',
  userStore: new WebStorageStateStore({ store: sessionStorage }),
  automaticSilentRenew: true,
})
```

### Auth Store (Pinia)

```typescript
// apps/shell/src/stores/auth.ts
export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)
  const isAuthenticated = computed(() => !!user.value && !user.value.expired)
  const accessToken = computed(() => user.value?.access_token ?? null)

  async function login() { await userManager.signinRedirect() }
  async function handleCallback() { user.value = await userManager.signinRedirectCallback() }
  async function logout() { await userManager.signoutRedirect(); user.value = null }
  async function getAccessToken(): Promise<string | null> {
    if (!user.value || user.value.expired) {
      user.value = await userManager.signinSilent()
    }
    return user.value?.access_token ?? null
  }

  return { user, isAuthenticated, accessToken, login, handleCallback, logout, getAccessToken }
})
```

### HTTP Client with Tenant Headers

```typescript
// apps/shell/src/plugins/http.ts
const client = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
})

// Auth header
client.interceptors.request.use(async (config) => {
  const token = await authStore.getAccessToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Tenant headers
client.interceptors.request.use((config) => {
  if (tenantStore.organizationId) config.headers['X-Organization-Id'] = tenantStore.organizationId
  if (tenantStore.workspaceId) config.headers['X-Workspace-Id'] = tenantStore.workspaceId
  return config
})

// 401 handler
client.interceptors.response.use((r) => r, async (error) => {
  if (error.response?.status === 401) await authStore.logout()
  return Promise.reject(error)
})
```

### Auth Flow Summary

1. User visits `/` -> router guard redirects to `/login`
2. `LoginPage` calls `authStore.login()` -> Keycloak redirect (PKCE)
3. Keycloak authenticates -> redirects to `/callback`
4. `AuthCallback` calls `authStore.handleCallback()` -> token stored in sessionStorage
5. Router navigates to `/select-organization`
6. `OrganizationPickerPage` fetches `GET /api/v1/users/me/organizations`
7. Single org: auto-select. Multiple: show picker. Zero: show message.
8. Selected org stored in `useTenantStore()` -> all API calls include `X-Organization-Id`

### Key Files

| File | Purpose |
|------|---------|
| `apps/shell/src/plugins/auth.ts` | oidc-client-ts UserManager setup |
| `apps/shell/src/stores/auth.ts` | Auth state (Pinia) |
| `apps/shell/src/stores/tenant.ts` | Org/workspace context (Pinia) |
| `apps/shell/src/plugins/http.ts` | Axios with auth + tenant interceptors |
| `apps/shell/src/router/index.ts` | Route guards (requiresAuth, requiresOrganization) |
