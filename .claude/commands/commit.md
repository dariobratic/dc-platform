---
description: Smart commit - tests, changelog, version bump, push
---

# Smart Commit Workflow

This command runs WITHOUT parameters. It auto-generates everything.

## Steps to Execute

### 1. Pre-commit Validation — Run Unit Tests

Run unit tests (exclude integration tests that need Docker):

```
dotnet test "dc-platform.slnx" --filter "Category!=Integration" --no-restore --verbosity minimal
```

- If tests FAIL → STOP immediately. Show the failure output and tell the user to fix tests first.
- If tests PASS → proceed to step 2.

### 2. Analyze Changes

Run `git status` and `git diff` (both staged and unstaged).

- If there are NO changes (clean working tree) → STOP and inform user "Nothing to commit."
- If there ARE changes → proceed to step 3.

### 3. Auto-generate Commit Message

Analyze the changed files to determine the commit type and scope.

**Determine type:**
- New files added → `feat`
- Files modified with bug fixes (look at context/description) → `fix`
- Test files only → `test`
- Documentation files only (*.md) → `docs`
- Refactoring (no new functionality, no bug fix) → `refactor`
- Dependencies, configs, tooling → `chore`

**Determine scope:**
- Changes in `services/directory/` → `directory`
- Changes in `services/access-control/` → `access-control`
- Changes in `services/audit/` → `audit`
- Changes in `services/authentication/` → `authentication`
- Changes in `services/notification/` → `notification`
- Changes in `services/configuration/` → `configuration`
- Changes in `services/gateway/` → `gateway`
- Changes in `services/admin-api/` → `admin-api`
- Changes in `apps/` → `frontend`
- Changes in `packages/` → `packages`
- Changes in `infrastructure/` → `infra`
- Changes in `.claude/` or root config files → `platform`
- Changes spanning multiple services → `platform`

**Generate description:**
- Short, lowercase, imperative mood (e.g., "add workspace membership API")
- Summarize WHAT changed, not HOW

Format: `type(scope): description`

### 4. Update CHANGELOG.md and VERSION

**Read current `VERSION` file** to get `MAJOR.BUILD` (e.g., `0.15`).

**Increment BUILD** by 1 (e.g., `0.15` → `0.16`).

**Write new VERSION** file with the incremented version.

**Prepend new entry to CHANGELOG.md** after the `---` line that follows the format header:

```markdown
## [NEW_VERSION] - YYYY-MM-DD

### Added/Fixed/Changed (pick the appropriate section based on commit type)
- Description of what changed (can be multiple bullet points)

---
```

Use today's date. Map commit type to changelog section:
- `feat` → `### Added`
- `fix` → `### Fixed`
- `refactor`, `chore` → `### Changed`
- `test` → `### Added` (with "Tests:" prefix)
- `docs` → `### Changed` (with "Documentation:" prefix)

### 5. Commit and Push

```bash
git add .
git commit -m "type(scope): description"
git push
```

- Do NOT add Co-Authored-By or any trailers
- Do NOT modify the auto-generated message

### 6. Summary

Show a clean summary:

```
✓ Version: 0.XX
✓ Commit: type(scope): description
✓ Files: N files changed
✓ Pushed to: branch-name
```

## Important Rules

- NEVER add Co-Authored-By or any other trailers
- NEVER ask the user for the commit message — always auto-generate it
- NEVER skip the test step
- If tests fail, STOP — do not commit broken code
- Commit message MUST follow Conventional Commits: `type(scope): description`
- Valid types: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`
- Valid scopes: service names, `platform`, `frontend`, `packages`, `infra`
