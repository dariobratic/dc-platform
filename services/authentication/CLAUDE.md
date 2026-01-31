# Authentication Service

OAuth2 token operations and user registration service for DC Platform. Handles authorization code exchange, token refresh, logout, custom signin/signup with Keycloak, and signup orchestration across Keycloak + Directory service.

## Service Scope

### This Service IS Responsible For:
- Exchanging authorization codes for access/refresh tokens
- Refreshing expired access tokens
- Revoking refresh tokens on logout
- Providing user information from JWT token claims
- Custom signin via Resource Owner Password Credentials (ROPC) flow
- Custom signup orchestration (Keycloak user creation + Directory org/workspace setup)
- Keycloak user attribute management (organizationId, tenantId)

### This Service IS NOT Responsible For:
- Session storage (stateless token-based auth)
- Password reset flows (→ Keycloak)
- Multi-factor authentication logic (→ Keycloak)
- Business logic or data persistence (no database)
- Organization/workspace CRUD beyond initial signup (→ Directory Service)

## Architecture

The Authentication service is a **stateless API service** that acts as a bridge between frontend applications and Keycloak. For signin/signup, it also orchestrates calls to the Directory service to set up organizations and workspaces.

### Key Characteristics:
- **No database** - Stateless, token-based authentication only
- **No Clean Architecture layers** - Single API project
- **Keycloak proxy** - Simplifies OAuth2 flow for frontend clients
- **Signup orchestrator** - Coordinates Keycloak + Directory for new user registration
- **JWT validation** - Validates tokens issued by Keycloak

## API Endpoints

All endpoints use route prefix `/api/v1/auth`.

### Token Operations

#### POST /api/v1/auth/token
Exchange authorization code for tokens (standard OAuth2 code flow).

**Request:**
```json
{
  "code": "auth-code-from-keycloak",
  "redirectUri": "http://localhost:3000/callback"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbG...",
  "refreshToken": "eyJhbG...",
  "expiresIn": 300,
  "tokenType": "Bearer"
}
```

**Errors:** `400` invalid code, `500` Keycloak error

#### POST /api/v1/auth/refresh
Refresh an expired access token.

**Request:**
```json
{
  "refreshToken": "eyJhbG..."
}
```

**Response (200 OK):** Same `TokenResponse` shape as token endpoint.

**Errors:** `400` invalid/expired refresh token, `500` Keycloak error

#### POST /api/v1/auth/logout
Revoke refresh token (best-effort — always returns success).

**Request:**
```json
{
  "refreshToken": "eyJhbG..."
}
```

**Response:** `204 No Content`

#### GET /api/v1/auth/userinfo
Get current user information from JWT token claims. Requires `Authorization: Bearer` header.

**Response (200 OK):**
```json
{
  "sub": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "email": "user@example.com",
  "emailVerified": true,
  "preferredUsername": "john.doe"
}
```

**Errors:** `401` missing/invalid token

### Custom Authentication

#### POST /api/v1/auth/signin
Authenticate with email and password using ROPC flow (bypasses Keycloak login page).

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Response (200 OK):** Same `TokenResponse` shape.

**Errors:** `401` invalid credentials, `429` account locked/rate limited, `500` unexpected error

#### POST /api/v1/auth/signup
Register a new user with organization setup. This is a multi-step orchestration endpoint.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123",
  "firstName": "John",
  "lastName": "Doe",
  "organizationName": "My Company"
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbG...",
  "refreshToken": "eyJhbG...",
  "expiresIn": 300,
  "tokenType": "Bearer",
  "userId": "guid",
  "organizationId": "guid",
  "workspaceId": "guid"
}
```

**Errors:** `409` email already exists or org slug conflict, `500` unexpected error

**Signup Orchestration Steps:**
1. Create user in Keycloak (Admin API)
2. Authenticate new user to get tokens (ROPC)
3. Create organization in Directory service (slug auto-generated from name)
4. Create default "General" workspace in organization
5. Add user as "Owner" to workspace membership
6. Update Keycloak user attributes with `organizationId` and `tenantId`
7. Re-authenticate to get fresh token with org claims

On partial failure after user creation, the orphaned Keycloak user is logged but not deleted.

### Health & Status
- `GET /health` - ASP.NET Core standard health check
- `GET /api/health` - Detailed health response with service info

## Configuration

### Keycloak Settings (`appsettings.json`)

```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/dc-platform",
    "Audience": "dc-platform-gateway",
    "RequireHttps": false,
    "BaseUrl": "http://localhost:8080",
    "Realm": "dc-platform",
    "ClientId": "dc-platform-gateway",
    "ClientSecret": "your-client-secret-here",
    "FrontendClientId": "dc-platform-admin"
  }
}
```

- `ClientId` / `ClientSecret` - Backend confidential client (code flow)
- `FrontendClientId` - Public client used for ROPC signin/signup flows

### Service Dependencies (`appsettings.json`)

```json
{
  "Services": {
    "DirectoryUrl": "http://localhost:5001"
  }
}
```

### CORS Settings

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

## Project Structure

```
services/authentication/
├── src/
│   └── Authentication.API/
│       ├── Controllers/
│       │   └── AuthController.cs          # All 6 auth endpoints
│       ├── Models/
│       │   ├── TokenRequest.cs            # Code exchange request
│       │   ├── TokenResponse.cs           # Standard token response
│       │   ├── RefreshTokenRequest.cs      # Refresh request
│       │   ├── LogoutRequest.cs           # Logout request
│       │   ├── UserInfoResponse.cs        # JWT claims response
│       │   ├── SigninRequest.cs           # Email + password
│       │   ├── SignupRequest.cs           # Email + password + name + org
│       │   ├── SignupResponse.cs          # Tokens + userId + orgId + wsId
│       │   ├── KeycloakTokenResponse.cs   # Internal Keycloak response mapping
│       │   ├── DirectoryModels.cs         # Directory service DTOs
│       │   └── HealthResponse.cs          # Health check response
│       ├── Services/
│       │   ├── IKeycloakService.cs        # Keycloak operations interface
│       │   ├── KeycloakService.cs         # Token exchange, user creation, ROPC
│       │   ├── IDirectoryService.cs       # Directory operations interface
│       │   └── DirectoryService.cs        # Org/workspace/member creation
│       ├── Middleware/
│       │   ├── CorrelationIdMiddleware.cs
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Authentication.API.csproj
│       └── Program.cs
│
├── Authentication.slnx
├── CLAUDE.md
└── README.md
```

## Downstream Dependencies

| Service | How Used |
|---------|----------|
| Keycloak (port 8080) | Token exchange, user creation (Admin API), attribute updates, ROPC auth |
| Directory (port 5001) | Org/workspace/member creation during signup |

### Keycloak Admin API Endpoints Used
- `POST /admin/realms/{realm}/users` - Create new user
- `PUT /admin/realms/{realm}/users/{id}` - Update user attributes
- `POST /realms/{realm}/protocol/openid-connect/token` - Token exchange + ROPC
- `POST /realms/{realm}/protocol/openid-connect/revoke` - Token revocation

### Directory Service Endpoints Called
- `POST /api/v1/organizations` - Create organization
- `POST /api/v1/organizations/{id}/workspaces` - Create workspace
- `POST /api/v1/workspaces/{id}/members` - Add member

## Coding Rules for This Service

1. **Stateless Only**: Never store tokens or user state in memory or cache
2. **Error Mapping**: Map Keycloak HTTP errors to appropriate API responses
3. **Logging**: Log all auth events for security auditing (never log tokens/passwords)
4. **Best-Effort Logout**: Token revocation failures should not block logout
5. **Claims-Based UserInfo**: Extract user info from validated JWT claims
6. **Typed HttpClients**: Use `IKeycloakService` and `IDirectoryService` interfaces
7. **Slug Generation**: Organization slugs generated from name using `GenerateSlug()` utility

## Security Considerations

1. **Client Secret Protection**: Never commit real secrets — use environment variables
2. **HTTPS in Production**: Set `RequireHttps: true`
3. **ROPC Flow**: Only used for custom signin/signup, not for general auth
4. **Keycloak Admin Credentials**: Required for user creation — protect in production
5. **CORS Configuration**: Only allow trusted frontend origins
6. **Token Expiration**: Access tokens 5 minutes, refresh tokens are long-lived

## Logging

Structured JSON logging via Serilog (see `.claude/skills/structured-logging/SKILL.md`).

- **Log output**: Console (structured text) + File (JSON)
- **File path**: `infrastructure/logs/authentication/log-{date}.json`
- **Rotation**: Daily, 30-day retention
- **Correlation ID**: All requests tagged via `X-Correlation-Id` header

## Commands

```bash
# From services/authentication/
dotnet restore
dotnet build
dotnet run --project src/Authentication.API

# Run on default port (5002)
dotnet run --project src/Authentication.API --urls "http://localhost:5002"
```

## Development Notes

- Default port: `5002` (HTTP)
- Health endpoint: `http://localhost:5002/api/health`
- No migrations or database setup required
- Keycloak must be running on `http://localhost:8080` with `dc-platform` realm
- Directory service must be running on `http://localhost:5001` for signup flow

## Testing the Service

### Prerequisites
1. Keycloak running on port 8080
2. Realm `dc-platform` created with auto-import from `infrastructure/keycloak/realm-export.json`
3. Client `dc-platform-gateway` (confidential) and `dc-platform-admin` (public) configured
4. Directory service running on port 5001 (for signup)

### Manual Testing

```bash
# Health check
curl http://localhost:5002/api/health

# Signin (ROPC)
curl -X POST http://localhost:5002/api/v1/auth/signin \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}'

# Signup (creates user + org + workspace)
curl -X POST http://localhost:5002/api/v1/auth/signup \
  -H "Content-Type: application/json" \
  -d '{"email":"new@example.com","password":"Password1!","firstName":"John","lastName":"Doe","organizationName":"My Company"}'

# Token exchange (requires valid auth code)
curl -X POST http://localhost:5002/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"code":"AUTH_CODE","redirectUri":"http://localhost:3000/callback"}'

# User info (requires valid access token)
curl http://localhost:5002/api/v1/auth/userinfo \
  -H "Authorization: Bearer ACCESS_TOKEN"
```

## Related Services

- **Gateway** (port 5000) - Routes `/api/v1/auth/**` to this service
- **Keycloak** (port 8080) - Identity provider, user storage
- **Directory** (port 5001) - Organization/workspace creation during signup
