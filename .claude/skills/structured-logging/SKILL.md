---
name: structured-logging
description: |
  Serilog structured logging patterns. Use when adding logging to services,
  configuring log levels, correlation IDs, or setting up log file output.
---
# Structured Logging - DC Platform

All DC Platform services use structured JSON logging via Serilog. This ensures consistent, queryable logs across the entire platform.

---

## Log Format

Every log entry is a JSON object with required and optional fields.

### Required Fields

| Field | Type | Source | Description |
|-------|------|--------|-------------|
| `timestamp` | `string` | Serilog | ISO 8601 UTC timestamp |
| `level` | `string` | Serilog | `Error`, `Warning`, `Information`, `Debug` |
| `service` | `string` | Config | Service name (e.g., `"Directory"`, `"AccessControl"`) |
| `correlationId` | `string` | Middleware | Request correlation ID for distributed tracing |
| `message` | `string` | Code | Human-readable log message |

### Optional Context Fields

| Field | Type | Source | Description |
|-------|------|--------|-------------|
| `userId` | `string` | JWT `sub` claim | Authenticated user ID |
| `organizationId` | `string` | `X-Organization-Id` header | Active organization context |
| `workspaceId` | `string` | `X-Workspace-Id` header | Active workspace context |
| `requestPath` | `string` | HttpContext | HTTP request path |
| `requestMethod` | `string` | HttpContext | HTTP method (GET, POST, etc.) |
| `statusCode` | `int` | HttpContext | HTTP response status code |
| `duration` | `double` | Middleware | Request duration in milliseconds |

### Exception Fields

When logging exceptions, include:

| Field | Type | Description |
|-------|------|-------------|
| `exceptionType` | `string` | Exception class name |
| `exceptionMessage` | `string` | Exception message |
| `stackTrace` | `string` | Full stack trace |
| `requestBody` | `string` | Request body (sanitized, no secrets) |

### Example Log Entry

```json
{
  "timestamp": "2025-01-29T14:30:00.123Z",
  "level": "Information",
  "service": "Directory",
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "message": "Organization created: Acme Corp",
  "userId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "organizationId": "b8f3c2d1-4e5a-6789-0abc-def123456789",
  "requestPath": "/api/v1/organizations",
  "requestMethod": "POST",
  "statusCode": 201,
  "duration": 45.2
}
```

### Example Error Entry

```json
{
  "timestamp": "2025-01-29T14:31:00.456Z",
  "level": "Error",
  "service": "Directory",
  "correlationId": "c3d4e5f6-7890-abcd-ef12-34567890abcd",
  "message": "Failed to create organization",
  "userId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "requestPath": "/api/v1/organizations",
  "requestMethod": "POST",
  "exceptionType": "Npgsql.PostgresException",
  "exceptionMessage": "23505: duplicate key value violates unique constraint",
  "stackTrace": "at Npgsql.Internal...",
  "requestBody": "{\"name\":\"Acme Corp\",\"slug\":\"acme\"}"
}
```

---

## Log Level Guidelines

### Error
Unhandled exceptions, infrastructure failures, data corruption risks. Requires investigation.

```csharp
_logger.LogError(ex, "Failed to save organization {OrganizationId}", organization.Id);
_logger.LogError("Database connection lost for service {Service}", "Directory");
_logger.LogError(ex, "Keycloak token exchange failed for redirect {RedirectUri}", redirectUri);
```

### Warning
Unexpected but handled situations. No immediate action required but worth monitoring.

```csharp
_logger.LogWarning("Invalid authorization code provided by client {IpAddress}", ipAddress);
_logger.LogWarning("Slug conflict detected for {Slug}, returning 409", slug);
_logger.LogWarning("Token revocation failed, token will expire naturally. TokenHint: {Hint}", "refresh_token");
_logger.LogWarning("Request took {Duration}ms, exceeding threshold", duration);
```

### Information
Business events that record what the system did. Useful for audit trails and monitoring.

```csharp
_logger.LogInformation("Organization created: {OrganizationName} ({OrganizationId})", name, id);
_logger.LogInformation("Member {UserId} added to workspace {WorkspaceId} with role {Role}", userId, wsId, role);
_logger.LogInformation("Role {RoleName} assigned to user {UserId} in scope {ScopeId}", roleName, userId, scopeId);
_logger.LogInformation("User {UserId} logged out", userId);
```

### Debug
Technical details for troubleshooting. Disabled in production by default.

```csharp
_logger.LogDebug("Querying organizations with filter: Slug={Slug}", slug);
_logger.LogDebug("Token validation parameters: Issuer={Issuer}, Audience={Audience}", issuer, audience);
_logger.LogDebug("EF query executed in {Duration}ms: {Query}", duration, query);
```

---

## NuGet Packages

```xml
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
<PackageReference Include="Serilog.Expressions" Version="5.0.0" />
```

---

## Serilog Configuration

### Program.cs Integration

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Replace default logging with Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

// ... other service registrations ...

var app = builder.Build();

// Middleware pipeline - ORDER IS CRITICAL
app.UseMiddleware<CorrelationIdMiddleware>();     // 1. First: assign correlation ID
app.UseMiddleware<ExceptionHandlingMiddleware>(); // 2. Second: catch all exceptions
app.UseSerilogRequestLogging(options =>           // 3. Third: log HTTP requests
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value);

        var userId = httpContext.User?.FindFirst("sub")?.Value;
        if (userId != null)
            diagnosticContext.Set("UserId", userId);

        var orgId = httpContext.Request.Headers["X-Organization-Id"].FirstOrDefault();
        if (orgId != null)
            diagnosticContext.Set("OrganizationId", orgId);

        var wsId = httpContext.Request.Headers["X-Workspace-Id"].FirstOrDefault();
        if (wsId != null)
            diagnosticContext.Set("WorkspaceId", wsId);
    };
});
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Correlation ID Middleware

Add before other middleware to ensure all logs include the correlation ID:

```csharp
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```

### appsettings.json Configuration

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.File", "Serilog.Expressions"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "Microsoft.EntityFrameworkCore.Database.Command": "Warning",
        "System.Net.Http.HttpClient": "Warning"
      }
    },
    "Enrich": ["FromLogContext"],
    "Properties": {
      "Service": "Directory"
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": {
            "type": "Serilog.Templates.ExpressionTemplate, Serilog.Expressions",
            "template": "{@t:yyyy-MM-dd HH:mm:ss.fff} [{@l:u3}] {Service} [{CorrelationId}] {@m}\n{@x}"
          }
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "../../../infrastructure/logs/directory/log-.json",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "formatter": {
            "type": "Serilog.Templates.ExpressionTemplate, Serilog.Expressions",
            "template": "{ {timestamp: @t, level: @l, service: Service, correlationId: CorrelationId, message: @m, userId: UserId, organizationId: OrganizationId, workspaceId: WorkspaceId, requestPath: RequestPath, requestMethod: RequestMethod, statusCode: StatusCode, duration: Elapsed, exceptionType: @x?.GetType().Name, exceptionMessage: @x?.Message, stackTrace: @x?.ToString(), ..@p} }\n"
          }
        }
      }
    ]
  }
}
```

### appsettings.Development.json Override

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.AspNetCore": "Information",
        "Microsoft.EntityFrameworkCore.Database.Command": "Information"
      }
    }
  }
}
```

### Per-Service Configuration

Each service changes only the `Service` property and the file path:

| Service | `Properties.Service` | File Path |
|---------|----------------------|-----------|
| Gateway | `"Gateway"` | `infrastructure/logs/gateway/log-.json` |
| Authentication | `"Authentication"` | `infrastructure/logs/authentication/log-.json` |
| Directory | `"Directory"` | `infrastructure/logs/directory/log-.json` |
| AccessControl | `"AccessControl"` | `infrastructure/logs/access-control/log-.json` |
| Audit | `"Audit"` | `infrastructure/logs/audit/log-.json` |
| Notification | `"Notification"` | `infrastructure/logs/notification/log-.json` |
| Configuration | `"Configuration"` | `infrastructure/logs/configuration/log-.json` |
| AdminAPI | `"AdminAPI"` | `infrastructure/logs/admin-api/log-.json` |

---

## File Output

### Directory Structure

```
infrastructure/
└── logs/
    ├── gateway/
    │   ├── log-20250129.json
    │   └── log-20250130.json
    ├── authentication/
    │   └── log-20250129.json
    ├── directory/
    │   └── log-20250129.json
    ├── access-control/
    │   └── log-20250129.json
    ├── audit/
    │   └── log-20250129.json
    └── ...
```

### Retention Policy

- **Rotation**: Daily (new file per day)
- **Retention**: 30 days (older files automatically deleted)
- **Format**: One JSON object per line (newline-delimited JSON)

---

## Logging Best Practices

### DO

- Use structured logging with named parameters: `_logger.LogInformation("Created {EntityType} {EntityId}", type, id)`
- Include entity IDs in log messages for traceability
- Log business events at `Information` level
- Log all exceptions with the `ex` parameter: `_logger.LogError(ex, "message")`
- Use `CorrelationId` to trace requests across services

### DO NOT

- Log sensitive data: passwords, tokens, secrets, personal data beyond userId/email
- Use string interpolation: `_logger.LogInformation($"Created {id}")` — this defeats structured logging
- Log at `Information` level in tight loops (use `Debug`)
- Swallow exceptions without logging: always log before handling
- Log request/response bodies in production (only in development via `Debug` level)

### Sanitization Rules

Before logging request bodies, strip these fields:

```csharp
private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
{
    "password", "secret", "token", "accessToken", "refreshToken",
    "clientSecret", "authorization", "cookie"
};
```

---

## Middleware Pipeline Order

**CRITICAL: Order matters for proper logging!**

```csharp
app.UseMiddleware<CorrelationIdMiddleware>();     // 1. Assign correlation ID FIRST
app.UseMiddleware<ExceptionHandlingMiddleware>(); // 2. Catch exceptions BEFORE Serilog
app.UseSerilogRequestLogging();                   // 3. Log HTTP requests (includes exceptions)
app.UseCors();                                    // 4. CORS
app.UseAuthentication();                          // 5. Auth
app.UseAuthorization();                           // 6. Authorization
app.MapControllers();                             // 7. Routing
```

**Why this order?**
- CorrelationId must be first so ALL logs have it
- ExceptionHandling before Serilog ensures exceptions are caught and logged properly
- Serilog logs the final status code after exception handling converts it to 4xx/5xx
