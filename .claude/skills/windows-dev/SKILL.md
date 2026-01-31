---
name: windows-dev
description: |
  Windows/PowerShell compatibility for DC Platform. Use when running shell commands,
  curl requests, Docker operations, or scripting on Windows. Detect the shell
  environment and adjust commands accordingly.
---
# Windows / PowerShell Compatibility

## Shell Detection

Before running commands, detect the environment:

```
# In Bash/Git Bash: $SHELL is set, $PSVersionTable is not
# In PowerShell: $PSVersionTable exists, $IsWindows is $true
# Platform check: process.platform === 'win32' (Node), Environment.OSVersion (C#)
```

The Claude Code CLI on Windows uses **Git Bash** as its shell. This means most Unix
commands work, but some edge cases with piping, quoting, and subshells behave
differently from native Linux Bash.

## Git Bash on Windows Quirks

Git Bash is the primary shell used by Claude Code on Windows. It is NOT PowerShell,
but it has its own compatibility issues.

### Path Translation (MSYS_NO_PATHCONV)

Git Bash auto-translates Unix-style paths to Windows paths. This breaks Docker
exec commands and URLs that look like paths.

```bash
# BROKEN - Git Bash translates /etc/nginx to C:/Program Files/Git/etc/nginx
docker exec container cat /etc/nginx/conf.d/default.conf

# CORRECT - Use sh -c to prevent path translation
docker exec container sh -c "cat /etc/nginx/conf.d/default.conf"

# CORRECT - Set MSYS_NO_PATHCONV=1 for single command
MSYS_NO_PATHCONV=1 docker exec container cat /etc/nginx/conf.d/default.conf
```

### Piping curl to python

Piping JSON through multiple commands can fail when stdin is consumed by the
shell. Use temp files or single-command processing.

```bash
# UNRELIABLE - Pipe chain with variable substitution can lose stdin
TOKEN=$(curl -s ... | python -c "...") && curl -s ... | python -c "..."

# RELIABLE - Write to file, then process
curl -s http://localhost:8080/endpoint > /tmp/response.json
python -c "import json; print(json.load(open('/tmp/response.json'))['key'])"

# RELIABLE - Use a single python script for complex processing
python -c "
import subprocess, json
r = subprocess.run(['curl', '-s', 'http://localhost:8080/endpoint'], capture_output=True)
data = json.loads(r.stdout)
print(data['key'])
"
```

### Temp File Paths

```bash
# /tmp does NOT exist on Windows. Use a project-relative path or $TEMP
# BROKEN
curl -s http://example.com > /tmp/output.json

# CORRECT - use project directory or Windows temp
curl -s http://example.com > ./tmp_output.json
# Clean up after
rm -f ./tmp_output.json
```

### Single Quotes in curl JSON

Git Bash handles single quotes differently from Linux Bash in some contexts,
especially with nested quotes.

```bash
# Usually works in Git Bash
curl -s -X POST http://localhost:5000/api -H "Content-Type: application/json" \
  -d '{"key":"value"}'

# If single quotes fail, use escaped double quotes
curl -s -X POST http://localhost:5000/api -H "Content-Type: application/json" \
  -d "{\"key\":\"value\"}"

# For complex JSON, use a heredoc or file
cat <<'EOF' > ./tmp_payload.json
{"key": "value", "nested": {"a": 1}}
EOF
curl -s -X POST http://localhost:5000/api -H "Content-Type: application/json" \
  -d @./tmp_payload.json
rm -f ./tmp_payload.json
```

## Docker Commands on Windows

### docker exec

```bash
# ALWAYS use sh -c for commands with paths (prevents Git Bash path translation)
docker exec dc-platform-admin sh -c "cat /etc/nginx/conf.d/default.conf"
docker exec dc-platform-postgres sh -c "psql -U dcplatform -d dc_platform -c 'SELECT 1;'"

# Without paths, direct commands work fine
docker exec dc-platform-admin wget --spider http://127.0.0.1/
```

### docker compose

```bash
# Works the same on Windows
docker compose -f infrastructure/docker-compose.yml up -d --build
docker compose -f infrastructure/docker-compose.yml down -v
docker compose -f infrastructure/docker-compose.yml logs -f service-name
```

### Health Checks in docker-compose.yml

```yaml
# Use 127.0.0.1 instead of localhost in wget health checks
# Alpine's wget resolves localhost to IPv6 [::1], but nginx only binds IPv4
healthcheck:
  test: ["CMD-SHELL", "wget -q --spider http://127.0.0.1/ || exit 1"]

# .NET services with curl (works fine with localhost)
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:5001/health"]
```

## PowerShell Reference (for users running commands manually)

If the user is running commands in PowerShell rather than Git Bash:

### Command Translation

| Bash | PowerShell |
|------|------------|
| `cat file` | `Get-Content file` |
| `grep pattern file` | `Select-String -Pattern pattern file` |
| `ls` | `Get-ChildItem` |
| `rm file` | `Remove-Item file` |
| `echo "text"` | `Write-Output "text"` |
| `export VAR=value` | `$env:VAR = "value"` |
| `cmd1 && cmd2` | `cmd1; if ($?) { cmd2 }` |
| `cmd1 \| cmd2` | `cmd1 \| cmd2` (same) |
| `$(command)` | `$(command)` (same in PS) |
| `curl` | `curl.exe` (PS aliases curl to Invoke-WebRequest) |

### Quoting

```powershell
# PowerShell uses double quotes for variable expansion
$name = "world"
Write-Output "Hello $name"        # Hello world
Write-Output 'Hello $name'        # Hello $name (literal)

# Escape with backtick, not backslash
Write-Output "Line1`nLine2"

# JSON in curl.exe
curl.exe -X POST http://localhost:5000/api -H "Content-Type: application/json" `
  -d '{\"key\":\"value\"}'
```

## DC Platform Patterns

### Multi-step API calls (preferred pattern)

```bash
# Use sequential commands with intermediate files for reliability
# Step 1: Get admin token
curl -s -X POST "http://localhost:8080/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin&password=admin&grant_type=password&client_id=admin-cli" \
  > ./tmp_token.json

TOKEN=$(python -c "import json; print(json.load(open('./tmp_token.json'))['access_token'])")

# Step 2: Use token
curl -s "http://localhost:8080/admin/realms/dc-platform/users" \
  -H "Authorization: Bearer $TOKEN" > ./tmp_users.json

# Step 3: Process result
python -c "import json; [print(u['id'], u.get('email','')) for u in json.load(open('./tmp_users.json'))]"

# Clean up
rm -f ./tmp_token.json ./tmp_users.json
```

### Database access

```bash
# Always use sh -c for psql commands in Docker
docker exec dc-platform-postgres sh -c \
  "psql -U dcplatform -d dc_platform -c \"SELECT * FROM directory.organizations;\""
```

### EF Core Migrations

```bash
# Run from the service directory using Git Bash
cd /c/Projects/dc-platform/services/directory
dotnet ef database update \
  --project src/Directory.Infrastructure \
  --startup-project src/Directory.API \
  -- --ConnectionStrings:DirectoryDb \
  "Host=localhost;Port=5432;Database=dc_platform;Username=dcplatform;Password=dcplatform_dev;Search Path=directory"
```

## Common Pitfalls

1. **Path translation** - Git Bash converts `/foo` to `C:/Program Files/Git/foo` in
   command arguments. Use `sh -c "..."` for docker exec or set `MSYS_NO_PATHCONV=1`.

2. **Temp files** - `/tmp` doesn't exist. Use project-relative paths and clean up.

3. **Complex pipe chains** - Long `cmd | python -c | cmd` chains are unreliable.
   Break into steps with intermediate files.

4. **curl response parsing** - Don't pipe curl directly to python in a subshell
   `$()` combined with other curl calls. The stdin gets mixed up.

5. **Line endings** - Git on Windows may use CRLF. Files created in containers
   use LF. Use `.gitattributes` to manage: `* text=auto eol=lf`.

6. **IPv6 in Alpine containers** - `localhost` resolves to `[::1]` in Alpine/BusyBox.
   Use `127.0.0.1` for health checks in nginx containers.

## Related Skills

- `docker-compose` — Container networking and health checks where path translation matters
- `troubleshooting` — Debug commands that need Windows-compatible alternatives
- `continuous-learning` — Capture platform-specific gotchas as learnings
