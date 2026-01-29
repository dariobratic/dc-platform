# Authentication Service

OAuth2 token operations service for DC Platform. Handles authorization code exchange, token refresh, and logout with Keycloak.

## Overview

The Authentication service acts as a bridge between frontend applications and Keycloak, simplifying the OAuth2 authorization code flow. It exchanges authorization codes for JWT tokens, refreshes expired tokens, and handles logout operations.

**Key Characteristics:**
- Stateless (no database)
- Proxy to Keycloak OAuth2 endpoints
- JWT token validation
- Single API project (no Clean Architecture layers)

## Prerequisites

Before running this service, ensure:

1. **Keycloak** is running on `http://localhost:8080`
2. Keycloak realm `dc-platform` is configured
3. Keycloak client `dc-platform-gateway` is set up with:
   - Client authentication enabled (confidential client)
   - Standard flow enabled (authorization code)
   - Valid redirect URIs configured (e.g., `http://localhost:3000/*`)
4. Client secret is configured in `appsettings.json` or environment variable

## Quick Start

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/Authentication.API

# Or run on specific port
dotnet run --project src/Authentication.API --urls "http://localhost:5002"
```

The service will start on `http://localhost:5002`.

## Configuration

### appsettings.json

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
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173"
    ]
  }
}
```

**Important:** Replace `your-client-secret-here` with the actual client secret from Keycloak. In production, use environment variables instead of hardcoding secrets.

### Environment Variables (Production)

```bash
export Keycloak__ClientSecret="your-actual-secret"
export Keycloak__RequireHttps="true"
export Keycloak__BaseUrl="https://your-keycloak-domain.com"
```

## API Endpoints

### POST /api/auth/token
Exchange authorization code for access and refresh tokens.

**Request:**
```json
{
  "code": "authorization-code-from-keycloak",
  "redirectUri": "http://localhost:3000/callback"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 300,
  "tokenType": "Bearer"
}
```

### POST /api/auth/refresh
Refresh an expired access token.

**Request:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response:** Same as token endpoint.

### GET /api/auth/userinfo
Get current user information (requires valid access token).

**Request Headers:**
```
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "sub": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "email": "user@example.com",
  "emailVerified": true,
  "preferredUsername": "john.doe"
}
```

### POST /api/auth/logout
Revoke refresh token and logout.

**Request:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response:** `204 No Content`

### GET /api/health
Health check with detailed response.

**Response:**
```json
{
  "serviceName": "Authentication",
  "status": "Healthy",
  "timestamp": "2026-01-29T12:00:00Z"
}
```

## OAuth2 Authorization Code Flow

This service implements the backend portion of the OAuth2 authorization code flow:

1. **Frontend initiates login:**
   - Redirect user to: `http://localhost:8080/realms/dc-platform/protocol/openid-connect/auth?client_id=dc-platform-gateway&redirect_uri=http://localhost:3000/callback&response_type=code&scope=openid`

2. **User authenticates with Keycloak:**
   - User enters credentials in Keycloak login page
   - Keycloak redirects back to: `http://localhost:3000/callback?code=AUTH_CODE`

3. **Frontend exchanges code for tokens:**
   - Frontend calls: `POST /api/auth/token` with authorization code
   - Service exchanges code with Keycloak
   - Returns access and refresh tokens

4. **Frontend uses access token:**
   - Include in API requests: `Authorization: Bearer {access_token}`
   - Token expires after 5 minutes (default)

5. **Frontend refreshes token when expired:**
   - Call: `POST /api/auth/refresh` with refresh token
   - Get new access token

6. **Logout:**
   - Call: `POST /api/auth/logout` with refresh token
   - Clear tokens from frontend storage

## Testing

### Health Check

```bash
curl http://localhost:5002/api/health
```

Expected response:
```json
{
  "serviceName": "Authentication",
  "status": "Healthy",
  "timestamp": "2026-01-29T..."
}
```

### Token Exchange (requires valid Keycloak authorization code)

```bash
curl -X POST http://localhost:5002/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{
    "code": "AUTH_CODE_FROM_KEYCLOAK",
    "redirectUri": "http://localhost:3000/callback"
  }'
```

### Get User Info (requires valid access token)

```bash
curl http://localhost:5002/api/auth/userinfo \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Project Structure

```
services/authentication/
├── src/
│   └── Authentication.API/
│       ├── Controllers/
│       │   └── AuthController.cs         # API endpoints
│       ├── Models/
│       │   ├── TokenRequest.cs           # Authorization code request
│       │   ├── TokenResponse.cs          # Token response
│       │   ├── RefreshTokenRequest.cs    # Refresh request
│       │   ├── LogoutRequest.cs          # Logout request
│       │   ├── UserInfoResponse.cs       # User info response
│       │   └── HealthResponse.cs         # Health check response
│       ├── Services/
│       │   ├── IKeycloakService.cs       # Keycloak service interface
│       │   └── KeycloakService.cs        # Keycloak HTTP client
│       ├── Properties/
│       │   └── launchSettings.json       # Launch configuration
│       ├── appsettings.json              # Configuration
│       ├── appsettings.Development.json  # Dev configuration
│       ├── Authentication.API.csproj     # Project file
│       └── Program.cs                    # Application entry point
├── Authentication.slnx                   # Solution file
├── CLAUDE.md                             # Developer guide
└── README.md                             # This file
```

## Development

This service is intentionally simple:
- No database or Entity Framework
- No Clean Architecture layers
- No domain logic
- Stateless token operations only

For detailed development guidelines, see [CLAUDE.md](./CLAUDE.md).

## Security Notes

1. **Never commit client secrets** - Use environment variables in production
2. **Enable HTTPS in production** - Set `Keycloak:RequireHttps: true`
3. **Secure token storage** - Frontend should store tokens securely (httpOnly cookies recommended)
4. **Short-lived access tokens** - Default 5-minute expiration
5. **CORS configuration** - Only allow trusted frontend origins

## Troubleshooting

### "Invalid client credentials" error
- Verify client secret in appsettings.json matches Keycloak
- Check that client authentication is enabled in Keycloak

### "Invalid grant" error on token exchange
- Authorization code may have expired (codes are single-use and short-lived)
- Verify redirect URI matches exactly what was used in authorization request

### "Unauthorized" on /api/auth/userinfo
- Access token may be expired (refresh it)
- Token may be invalid or from wrong issuer
- Check that Keycloak Authority URL is correct

### CORS errors from frontend
- Add frontend origin to `Cors:AllowedOrigins` in appsettings.json
- Verify frontend is sending credentials with requests

## Related Services

- **Gateway Service** - API Gateway (port 5000)
- **Keycloak** - Identity Provider (port 8080)
- **Directory Service** - Organization/Workspace management (port 5001)

## Support

For questions or issues, refer to:
- Platform documentation at `docs/`
- [Keycloak Documentation](https://www.keycloak.org/docs/latest/)
- [OAuth2 Specification](https://oauth.net/2/)
