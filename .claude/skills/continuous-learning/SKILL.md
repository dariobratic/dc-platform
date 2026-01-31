---
name: continuous-learning
description: |
  Pattern capture and knowledge retention. Use when Claude solves a non-trivial problem,
  discovers an undocumented behavior, or finds a workaround that should be remembered.
---

# Continuous Learning

## When to Use

- After solving a problem that took multiple debugging attempts
- When discovering undocumented behavior in a dependency (Keycloak, EF Core, Vite, etc.)
- When a workaround is needed for a tool/library limitation
- After fixing a recurring issue that keeps coming back
- When a pattern emerges across multiple similar tasks

## Learning Entry Format

Store entries in `.claude/learnings/` as markdown files.

**Filename**: `YYYY-MM-DD-{slug}.md`

```markdown
# Title: Short description of the pattern

## Problem
What went wrong or what was the challenge?
Include error messages, symptoms, or the misleading behavior.

## Solution
What fixed it? Include code snippets, commands, or config changes.
Be specific — future Claude sessions need to reproduce this.

## When to Use
Under what conditions should this pattern be applied?
What are the trigger signals that this learning is relevant?

## Related
- Files: list of files involved
- Skills: list of related skills
- Services: list of affected services
```

### Example

```markdown
# Title: Keycloak ROPC returns 401 not 400 for invalid credentials

## Problem
AuthController caught HttpRequestException for status 400 (BadRequest) when
Keycloak ROPC token endpoint returns invalid_grant. But Keycloak actually
returns HTTP 401 for invalid credentials, not 400.

## Solution
Catch both 400 and 401 in the AuthController's SignIn endpoint:
```csharp
catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
```

## When to Use
When handling Keycloak token endpoint errors in backend code.
Don't assume standard OAuth2 error codes — Keycloak has its own conventions.

## Related
- Files: services/authentication/src/Authentication.API/Controllers/AuthController.cs
- Skills: keycloak-integration, keycloak-admin, troubleshooting
- Services: authentication
```

## Capture Workflow

### Automatic (during work)

When Claude identifies a non-trivial solution during a task:

1. Complete the current task first
2. Assess if the pattern is reusable (not a one-off typo fix)
3. If yes, create a learning entry
4. Reference relevant skills and files

### Manual (via /learn command)

User triggers `/learn` after a complex debugging session:

1. Claude reviews the session context
2. Identifies the key problem → solution pattern
3. Creates a learning entry with full context
4. Suggests whether it should be promoted to a skill

## Promotion Criteria

A learning should be promoted to a skill section when:

| Criteria | Threshold |
|----------|-----------|
| Encountered multiple times | 3+ occurrences |
| Applies across services/apps | 2+ affected areas |
| Non-obvious workaround | Would surprise a new developer |
| Time cost when unknown | >15 minutes to figure out |

### Promotion Process

1. Identify the target skill (or create a new one)
2. Add the pattern to the relevant section of the skill
3. Keep the learning entry (mark it as "Promoted to: skill-name")
4. Update cross-references in related skills

## Review Process

Periodically (or when `/claude-sync` is run):

1. Read all entries in `.claude/learnings/`
2. Check for patterns that meet promotion criteria
3. Check for stale entries (problem no longer exists after refactoring)
4. Suggest promotions or archival to the user

## What NOT to Capture

- One-off typos or copy-paste mistakes
- Standard patterns already documented in skills
- Project setup steps (those belong in CLAUDE.md)
- Personal preferences or style choices
- Obvious fixes that any developer would know

## Related Skills

- `troubleshooting` — Systematic debugging that produces learnings
- `keycloak-admin` — Keycloak-specific gotchas worth capturing
- `windows-dev` — Platform-specific workarounds worth capturing
