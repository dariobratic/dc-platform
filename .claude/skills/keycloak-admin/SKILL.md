---
name: keycloak-admin
description: |
  Keycloak server configuration and troubleshooting. Use when modifying realm-export.json,
  configuring client scopes, fixing auth errors (invalid scopes, CORS, redirect URI),
  or debugging JWT tokens. For code integration patterns, see keycloak-integration skill.
---

# Keycloak Administration - DC Platform

> For code integration patterns (.NET JWT validation, Vue.js auth flow, middleware), see `.claude/skills/keycloak-integration/SKILL.md`.

## File Locations

| File | Purpose |
|------|---------|
| `infrastructure/keycloak/realm-export.json` | Realm definition, imported on first Keycloak start |
| `infrastructure/docker-compose.yml` | Keycloak container config (ports, env vars, volumes) |
| `infrastructure/.env` | `KEYCLOAK_ADMIN`, `KEYCLOAK_ADMIN_PASSWORD`, client secrets |
| `apps/shell/.env.local` | `VITE_KEYCLOAK_URL`, `VITE_KEYCLOAK_REALM`, `VITE_KEYCLOAK_CLIENT_ID` |

## Environment

- **Keycloak URL**: http://localhost:8080
- **Admin console**: http://localhost:8080/admin (admin / admin)
- **Realm**: `dc-platform`
- **OIDC discovery**: http://localhost:8080/realms/dc-platform/.well-known/openid-configuration
- **Keycloak version**: 26.1 (Quay.io image)

---

## realm-export.json Structure

The realm is imported on first Keycloak start via `--import-realm`. Import strategy is `IGNORE_EXISTING` -- if the realm already exists in PostgreSQL, the file is skipped.

### Top-Level Fields

```json
{
  "realm": "dc-platform",
  "enabled": true,
  "roles": { "realm": [...] },
  "clients": [...],
  "clientScopes": [...],
  "defaultDefaultClientScopes": [...],
  "defaultOptionalClientScopes": [...],
  "accessTokenLifespan": 300,
  "ssoSessionIdleTimeout": 1800,
  "ssoSessionMaxLifespan": 36000
}
```

### Realm Roles

```json
"roles": {
  "realm": [
    { "name": "platform-admin", "description": "Full platform access" },
    { "name": "org-admin", "description": "Organization administrator" },
    { "name": "org-member", "description": "Organization member with basic access" }
  ]
}
```

Realm roles are coarse-grained. Fine-grained authorization is handled by the Access Control service, not Keycloak.

---

## Clients

### dc-platform-gateway (Confidential)

Used by backend services for token validation. Not used directly by the frontend.

```json
{
  "clientId": "dc-platform-gateway",
  "publicClient": false,
  "clientAuthenticatorType": "client-secret",
  "secret": "change-me-in-production",
  "standardFlowEnabled": true,
  "directAccessGrantsEnabled": true,
  "serviceAccountsEnabled": true,
  "defaultClientScopes": ["openid", "profile", "email", "roles", "web-origins"],
  "optionalClientScopes": ["offline_access", "dc-platform-scope"],
  "redirectUris": ["http://localhost:3000/*", "http://localhost:5000/*", "http://localhost:5173/*"],
  "webOrigins": ["http://localhost:3000", "http://localhost:5000", "http://localhost:5173"]
}
```

Key properties:
- `publicClient: false` -- requires client secret for token exchange
- `serviceAccountsEnabled: true` -- allows machine-to-machine auth
- `protocolMappers` -- adds `tenant_id`, `organization_id`, `roles` claims to tokens

### dc-platform-admin (Public SPA)

Used by the Vue.js shell app for user login via Authorization Code + PKCE.

```json
{
  "clientId": "dc-platform-admin",
  "publicClient": true,
  "standardFlowEnabled": true,
  "directAccessGrantsEnabled": false,
  "defaultClientScopes": ["openid", "profile", "email", "roles", "web-origins"],
  "optionalClientScopes": ["offline_access", "dc-platform-scope"],
  "redirectUris": ["http://localhost:3000/*", "http://localhost:5173/*"],
  "webOrigins": ["http://localhost:3000", "http://localhost:5173"],
  "attributes": {
    "pkce.code.challenge.method": "S256",
    "post.logout.redirect.uris": "http://localhost:3000/*##http://localhost:5173/*"
  }
}
```

Key properties:
- `publicClient: true` -- no client secret (SPA cannot keep secrets)
- `pkce.code.challenge.method: S256` -- enforces PKCE for security
- `directAccessGrantsEnabled: false` -- no password grant (SPA should never handle passwords)
- `post.logout.redirect.uris` -- uses `##` as separator for multiple URIs

---

## Client Scopes

Every client MUST have `defaultClientScopes` explicitly listed in the realm export. Without this, Keycloak will not assign scopes to the client during import.

### Required Scope Definitions

The realm export must define these in the `clientScopes` array:

| Scope | Purpose | `include.in.token.scope` |
|-------|---------|--------------------------|
| `openid` | OIDC mandatory scope, includes `sub` claim | `true` |
| `profile` | User profile claims: `preferred_username`, `given_name`, `family_name` | `true` |
| `email` | Email claims: `email`, `email_verified` | `true` |
| `roles` | Realm and client role mappings | `false` |
| `web-origins` | Allowed CORS origins in token | `false` |
| `dc-platform-scope` | Custom DC Platform API scope | `true` |

### Realm-Level Defaults

```json
"defaultDefaultClientScopes": ["openid", "profile", "email", "roles", "web-origins"],
"defaultOptionalClientScopes": ["offline_access", "dc-platform-scope"]
```

`defaultDefaultClientScopes` applies to newly created clients. Existing clients in the export need their own `defaultClientScopes` array.

### Adding a New Scope

1. Define the scope object in the `clientScopes` array
2. Add protocol mappers if the scope adds token claims
3. Add the scope name to `defaultDefaultClientScopes` (for future clients)
4. Add the scope name to each existing client's `defaultClientScopes` array
5. Re-import the realm (see Applying Changes below)

---

## Protocol Mappers

Protocol mappers add custom claims to tokens. DC Platform uses these on the `dc-platform-gateway` client:

### tenant_id (User Attribute -> Token)

```json
{
  "name": "tenant-id",
  "protocolMapper": "oidc-usermodel-attribute-mapper",
  "config": {
    "user.attribute": "tenantId",
    "claim.name": "tenant_id",
    "id.token.claim": "true",
    "access.token.claim": "true",
    "userinfo.token.claim": "true",
    "jsonType.label": "String"
  }
}
```

### organization_id (User Attribute -> Token)

```json
{
  "name": "organization-id",
  "protocolMapper": "oidc-usermodel-attribute-mapper",
  "config": {
    "user.attribute": "organizationId",
    "claim.name": "organization_id",
    "id.token.claim": "true",
    "access.token.claim": "true",
    "userinfo.token.claim": "true",
    "jsonType.label": "String"
  }
}
```

### roles (Realm Roles -> Token)

```json
{
  "name": "realm-roles",
  "protocolMapper": "oidc-usermodel-realm-role-mapper",
  "config": {
    "multivalued": "true",
    "claim.name": "roles",
    "id.token.claim": "true",
    "access.token.claim": "true",
    "userinfo.token.claim": "true",
    "jsonType.label": "String"
  }
}
```

### Adding a New Protocol Mapper

Add to the client's `protocolMappers` array in realm-export.json:

```json
{
  "name": "my-custom-claim",
  "protocol": "openid-connect",
  "protocolMapper": "oidc-usermodel-attribute-mapper",
  "consentRequired": false,
  "config": {
    "user.attribute": "myAttribute",
    "claim.name": "my_claim",
    "id.token.claim": "true",
    "access.token.claim": "true",
    "userinfo.token.claim": "true",
    "jsonType.label": "String"
  }
}
```

Common mapper types:
- `oidc-usermodel-attribute-mapper` -- maps a user attribute to a claim
- `oidc-usermodel-realm-role-mapper` -- maps realm roles to a claim
- `oidc-usermodel-client-role-mapper` -- maps client roles to a claim
- `oidc-full-name-mapper` -- combines first/last name
- `oidc-allowed-origins-mapper` -- adds allowed CORS origins
- `oidc-sub-mapper` -- maps the subject claim

---

## Common Errors and Fixes

### "Invalid scopes: openid profile email"

**Cause**: The client does not have `openid`, `profile`, and `email` assigned as default client scopes.

**Fix in realm-export.json**:

1. Ensure `clientScopes` array contains scope definitions for `openid`, `profile`, and `email` with their protocol mappers
2. Ensure the client has `defaultClientScopes`:
   ```json
   "defaultClientScopes": ["openid", "profile", "email", "roles", "web-origins"]
   ```
3. Ensure `defaultDefaultClientScopes` is set at realm level
4. Re-import the realm (see Applying Changes below)

**Fix via Admin Console** (without re-import):
1. Go to http://localhost:8080/admin -> Realm: dc-platform
2. Clients -> dc-platform-admin -> Client scopes tab
3. Click "Add client scope" -> select openid, profile, email -> Add as Default

### "Invalid redirect_uri"

**Cause**: The redirect URI sent by the app doesn't match any pattern in the client's `redirectUris`.

**Fix**: Check `redirectUris` in realm-export.json:
```json
"redirectUris": [
  "http://localhost:3000/*",
  "http://localhost:5173/*"
]
```

Rules:
- Use `/*` wildcard suffix to match all paths under a host
- Must include scheme (`http://` or `https://`)
- Must match exactly (port included)
- For post-logout, check `attributes.post.logout.redirect.uris` (separator: `##`)

### CORS Errors ("blocked by CORS policy")

**Cause**: The app's origin is not in the client's `webOrigins`.

**Fix**: Add the origin to `webOrigins` (without trailing slash or path):
```json
"webOrigins": [
  "http://localhost:3000",
  "http://localhost:5173"
]
```

Alternatively, set `webOrigins: ["+"]` to derive allowed origins from `redirectUris` (not recommended for production).

### Token Claims Missing

**Cause**: Protocol mapper not configured, or mapper config has wrong `claim.name`.

**Debug**:
1. Decode the token (see JWT Debugging below)
2. Check if the claim exists in the decoded payload
3. If missing, verify the protocol mapper in realm-export.json:
   - `access.token.claim` must be `"true"` for the claim to appear in access tokens
   - `id.token.claim` must be `"true"` for ID tokens
   - `userinfo.token.claim` must be `"true"` for the userinfo endpoint
4. Verify the mapper is on the correct client (or in a client scope assigned to the client)

### "Account is not fully set up" After Login

**Cause**: Keycloak requires the user to complete required actions (verify email, update password, etc.).

**Fix**: In admin console, go to Users -> select user -> Required User Actions -> clear any pending actions.

---

## JWT Debugging

### Decode a Token

Paste the access token at https://jwt.io to inspect claims.

Or decode locally:

```bash
# Extract payload from a JWT (second segment, base64)
echo "YOUR_JWT_TOKEN" | cut -d. -f2 | base64 -d 2>/dev/null | python3 -m json.tool
```

### Expected Claims in DC Platform Token

```json
{
  "sub": "a1b2c3d4-...",
  "iss": "http://localhost:8080/realms/dc-platform",
  "aud": "account",
  "exp": 1706550000,
  "iat": 1706546400,
  "email": "user@example.com",
  "email_verified": true,
  "preferred_username": "john.doe",
  "given_name": "John",
  "family_name": "Doe",
  "tenant_id": "...",
  "organization_id": "...",
  "roles": ["org-member"]
}
```

### Verify via Keycloak Admin API

```bash
# Get admin token
curl -s -X POST "http://localhost:8080/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&client_id=admin-cli&username=admin&password=admin"

# List clients
curl -s -H "Authorization: Bearer $TOKEN" \
  "http://localhost:8080/admin/realms/dc-platform/clients?clientId=dc-platform-admin"

# List client's default scopes (use client UUID from above)
curl -s -H "Authorization: Bearer $TOKEN" \
  "http://localhost:8080/admin/realms/dc-platform/clients/$CLIENT_UUID/default-client-scopes"
```

### Keycloak Admin Console Navigation

| Task | Path |
|------|------|
| View realm settings | Realm settings (left nav) |
| View/edit clients | Clients -> select client |
| View client scopes | Clients -> client -> Client scopes tab |
| Add scope to client | Clients -> client -> Client scopes -> Add client scope |
| View users | Users -> select user |
| Set user attributes | Users -> user -> Attributes tab |
| Assign realm roles | Users -> user -> Role mapping tab |
| View realm roles | Realm roles (left nav) |
| Check events/logs | Events (left nav) -> Login events or Admin events |

---

## Applying Changes to realm-export.json

The import runs ONCE on first Keycloak start (strategy: `IGNORE_EXISTING`). To apply changes:

### Method 1: Clean Re-import (Development Only)

```bash
# 1. Drop Keycloak schema (realm data is in PostgreSQL, not a Docker volume)
docker exec dc-platform-postgres psql -U dcplatform -d dc_platform \
  -c "DROP SCHEMA IF EXISTS keycloak CASCADE; CREATE SCHEMA keycloak;"

# 2. Restart Keycloak to trigger fresh import
docker restart dc-platform-keycloak

# 3. Verify import succeeded (look for "Realm 'dc-platform' imported")
docker logs dc-platform-keycloak 2>&1 | grep -E "Realm|import"
```

This destroys all Keycloak data (users, sessions). Use only in development.

### Method 2: Admin Console (Preserve Data)

Make changes directly in the Keycloak admin console at http://localhost:8080/admin. Then update realm-export.json to match so the config is version-controlled.

### Method 3: Admin REST API

```bash
# Get admin token
TOKEN=$(curl -s -X POST "http://localhost:8080/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&client_id=admin-cli&username=admin&password=admin" \
  | grep -o '"access_token":"[^"]*"' | cut -d'"' -f4)

# Example: add a scope to a client
curl -X PUT "http://localhost:8080/admin/realms/dc-platform/clients/$CLIENT_UUID/default-client-scopes/$SCOPE_UUID" \
  -H "Authorization: Bearer $TOKEN"
```

---

## Creating a Test User

Via admin console:
1. Go to http://localhost:8080/admin -> Realm: dc-platform
2. Users -> Add user
3. Set username, email, first/last name
4. Credentials tab -> Set password (toggle "Temporary" off)
5. Attributes tab -> Add `tenantId` attribute if needed
6. Role mapping tab -> Assign `org-member` or `org-admin`

Via admin API:
```bash
curl -X POST "http://localhost:8080/admin/realms/dc-platform/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "firstName": "Test",
    "lastName": "User",
    "enabled": true,
    "emailVerified": true,
    "credentials": [{"type": "password", "value": "test1234", "temporary": false}],
    "attributes": {"tenantId": ["default-tenant"]}
  }'
```

---

## Keycloak Log Messages

| Log Message | Meaning |
|-------------|---------|
| `Realm 'dc-platform' imported` | Fresh import succeeded |
| `Realm 'dc-platform' already exists. Import skipped` | Realm data exists in PostgreSQL; realm-export.json changes NOT applied |
| `KC-SERVICES0050: Initializing master realm` | First-time Keycloak startup |
| `KC-SERVICES0077: Created temporary admin user` | Admin user created from env vars |
| `ISPN000556: Starting user marshaller` | Infinispan cache starting (normal) |

---

## Skill Maintenance

Update this skill when:
- A new error/fix pattern is discovered that took >5 minutes to solve
- New client or scope is added to dc-platform realm
- Keycloak version upgrade introduces config changes

Do NOT update for:
- One-off typos or config mistakes
- Issues already covered in existing sections
