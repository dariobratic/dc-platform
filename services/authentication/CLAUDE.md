# Authentication Service

OAuth2 token operations service for DC Platform. Handles authorization code exchange, token refresh, and logout with Keycloak.

## Service Scope

### This Service IS Responsible For:
- Exchanging authorization codes for access/refresh tokens
- Refreshing expired access tokens
- Revoking refresh tokens on logout
- Providing user information from JWT token claims
- Token validation for protected endpoints

### This Service IS NOT Responsible For:
- User registration or management (→ Keycloak)
- Session storage (stateless token-based auth)
- Password reset flows (→ Keycloak)
- Multi-factor authentication logic (→ Keycloak)
- Business logic or data persistence (no database)

## Architecture

The Authentication service is a **stateless API service** that acts as a bridge between frontend applications and Keycloak. It does not follow Clean Architecture patterns as it has no business logic or domain model.

### Key Characteristics:
- **No database** - Stateless, token-based authentication only
- **No Clean Architecture layers** - Single API project
- **Proxy to Keycloak** - Simplifies OAuth2 flow for frontend clients
- **JWT validation** - Validates tokens issued by Keycloak

## API Endpoints

### Authentication Flow

#### POST /api/auth/token
Exchange authorization code for tokens.

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
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 300,
  "tokenType": "Bearer"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid authorization code
- `500 Internal Server Error` - Keycloak communication error

#### POST /api/auth/refresh
Refresh an expired access token.

**Request:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 300,
  "tokenType": "Bearer"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid or expired refresh token
- `500 Internal Server Error` - Keycloak communication error

#### GET /api/auth/userinfo
Get current user information from JWT token.

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response (200 OK):**
```json
{
  "sub": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "email": "user@example.com",
  "emailVerified": true,
  "preferredUsername": "john.doe"
}
```

**Error Responses:**
- `401 Unauthorized` - Missing, invalid, or expired token
- `500 Internal Server Error` - Error reading token claims

#### POST /api/auth/logout
Revoke refresh token and logout user.

**Request:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response:**
- `204 No Content` - Token revoked successfully (or best-effort)

**Note:** This endpoint always returns success even if token revocation fails, as the token will expire naturally.

### Health & Status
- `GET /health` - Basic health check (ASP.NET Core standard)
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
    "ClientSecret": "your-client-secret-here"
  }
}
```

**Configuration Keys:**
- `Authority` - Keycloak realm URL for JWT validation
- `Audience` - Expected audience claim in JWT tokens
- `RequireHttps` - Set to `false` for local development, `true` in production
- `BaseUrl` - Keycloak server base URL
- `Realm` - Keycloak realm name
- `ClientId` - OAuth2 client ID (configured in Keycloak)
- `ClientSecret` - OAuth2 client secret (should be from environment variable in production)

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

Add your frontend application URLs to allow cross-origin requests.

## Project Structure

```
services/authentication/
├── src/
│   └── Authentication.API/        # ASP.NET Core Web API
│       ├── Controllers/           # AuthController
│       ├── Models/                # Request/Response DTOs
│       ├── Services/              # KeycloakService for HTTP calls
│       ├── Properties/            # Launch settings
│       ├── appsettings.json       # Configuration
│       ├── Authentication.API.csproj
│       └── Program.cs
│
├── Authentication.slnx
├── CLAUDE.md                      # This file
└── README.md
```

## Technical Requirements

### No Database
This service is stateless and does not persist any data. All authentication state is managed by Keycloak through JWT tokens.

### No Domain Logic
This service acts as a proxy/adapter for Keycloak's OAuth2 endpoints. It does not contain business logic.

### Dependencies
- `Microsoft.AspNetCore.OpenApi` - API documentation
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT token validation
- ASP.NET Core Health Checks for monitoring

## Keycloak Integration

### Token Endpoints Used
- **Token Exchange:** `POST /realms/{realm}/protocol/openid-connect/token`
  - Exchanges authorization code for tokens
  - Refreshes access tokens using refresh tokens
- **Token Revocation:** `POST /realms/{realm}/protocol/openid-connect/revoke`
  - Revokes refresh tokens on logout

### OAuth2 Flows

#### Authorization Code Flow
1. Frontend redirects user to Keycloak login page
2. User authenticates with Keycloak
3. Keycloak redirects back with authorization code
4. Frontend calls `POST /api/auth/token` with code
5. Service exchanges code for tokens with Keycloak
6. Frontend stores tokens and uses access token for API calls

#### Token Refresh Flow
1. Frontend detects access token expiration (or 401 response)
2. Frontend calls `POST /api/auth/refresh` with refresh token
3. Service requests new tokens from Keycloak
4. Frontend replaces old access token with new one

#### Logout Flow
1. Frontend calls `POST /api/auth/logout` with refresh token
2. Service revokes refresh token in Keycloak
3. Frontend clears stored tokens

## Coding Rules for This Service

1. **Stateless Only**: Never store tokens or user state in memory or cache
2. **No Business Logic**: Only token operations and claims extraction
3. **Error Handling**: Map Keycloak HTTP errors to appropriate API responses
4. **Logging**: Log authentication events for security auditing
5. **Best-Effort Logout**: Token revocation failures should not block logout
6. **Claims-Based UserInfo**: Extract user info from validated JWT claims, don't call Keycloak userinfo endpoint

## Security Considerations

1. **Client Secret Protection**: Never commit real secrets to git. Use environment variables in production.
2. **HTTPS in Production**: Set `RequireHttps: true` for production deployments
3. **Token Storage**: Access tokens should be stored securely by frontend (httpOnly cookies or secure storage)
4. **Token Expiration**: Access tokens are short-lived (5 minutes default), refresh tokens are long-lived
5. **CORS Configuration**: Only allow trusted frontend origins

## Logging

This service uses structured JSON logging via Serilog (see `.claude/skills/structured-logging/SKILL.md`).

- **Log output**: Console (structured text) + File (JSON)
- **File path**: `infrastructure/logs/authentication/log-{date}.json`
- **Rotation**: Daily, 30-day retention
- **Correlation ID**: All requests tagged via `X-Correlation-Id` header
- **Context enrichment**: RequestMethod, RequestPath, UserId, OrganizationId, WorkspaceId

See the structured-logging skill for log level guidelines and best practices.

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
- OpenAPI docs (dev only): `http://localhost:5002/openapi/v1.json`
- No migrations or database setup required
- No Clean Architecture setup needed (intentionally simple)
- Keycloak must be running on `http://localhost:8080` with `dc-platform` realm configured

## Testing the Service

### Prerequisites
1. Keycloak running on port 8080
2. Realm `dc-platform` created
3. Client `dc-platform-gateway` configured with:
   - Client authentication enabled (confidential client)
   - Standard flow enabled (authorization code flow)
   - Valid redirect URIs configured

### Manual Testing

1. **Health Check:**
```bash
curl http://localhost:5002/api/health
```

2. **Token Exchange** (requires valid authorization code from Keycloak):
```bash
curl -X POST http://localhost:5002/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"code":"AUTH_CODE","redirectUri":"http://localhost:3000/callback"}'
```

3. **Token Refresh:**
```bash
curl -X POST http://localhost:5002/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"REFRESH_TOKEN"}'
```

4. **User Info:**
```bash
curl http://localhost:5002/api/auth/userinfo \
  -H "Authorization: Bearer ACCESS_TOKEN"
```

5. **Logout:**
```bash
curl -X POST http://localhost:5002/api/auth/logout \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"REFRESH_TOKEN"}'
```

## Related Services

- **Gateway Service** - Routes requests to this service (port 5000)
- **Keycloak** - Identity provider (port 8080)
- **Directory Service** - User organization/workspace management (port 5001)

## Support

For questions or issues, refer to:
- Main platform documentation at `docs/`
- Keycloak documentation: https://www.keycloak.org/docs/latest/
- OAuth2 specification: https://oauth.net/2/
