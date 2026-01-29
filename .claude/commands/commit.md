---
description: Commit and push changes with validated message format
argument-hint: <type>(<scope>): <description>
---

# Git Commit Workflow

Commit message provided: `$ARGUMENTS`

## Steps to Execute

1. **Validate commit message format**
   - Must match: `type(scope): description`
   - Valid types: `feat`, `fix`, `refactor`, `docs`, `test`, `chore`
   - Scope should be service name (e.g., `directory`, `gateway`) or `platform`
   - If invalid, STOP and show the correct format

2. **Check for changes**
   - Run `git status`
   - If no changes, inform user and stop

3. **Stage and commit**
   - Run `git add .`
   - Run `git commit -m "$ARGUMENTS"`
   - Do NOT modify the commit message - use EXACTLY what user provided

4. **Push to remote**
   - Run `git push`

5. **Report summary**
   - Show files committed
   - Show current branch
   - Confirm push success

## Important Rules

- NEVER add Co-Authored-By or any other trailers
- NEVER modify the commit message
- Use the EXACT message from $ARGUMENTS
