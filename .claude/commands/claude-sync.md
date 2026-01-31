# Sync and Audit Claude Code Infrastructure

Scan all CLAUDE.md files, skills, agents, and learnings for staleness and consistency.

## Steps to Execute

### 1. Inventory All Files

Scan and list:
- All `CLAUDE.md` files (root + services + apps + packages)
- All skills in `.claude/skills/*/SKILL.md`
- All agents in `.claude/agents/*.md`
- All commands in `.claude/commands/*.md`
- All learnings in `.claude/learnings/*.md`

### 2. Audit CLAUDE.md Files

For each CLAUDE.md, check:

**a. Key Files Map**
- Do all listed files still exist on disk? Flag any that don't.
- Are there new important files not listed? (Check for new pages, services, components added recently via `git log --oneline -20 --name-only`)

**b. Routes and Endpoints**
- For app CLAUDE.md files: compare listed routes to actual `routes.ts` files
- For service CLAUDE.md files: compare listed endpoints to actual controllers

**c. Tech Stack Versions**
- Compare listed versions against `package.json` / `.csproj` files
- Flag any mismatches

**d. Decision Flow**
- Verify agent and skill recommendations in decision tables match what actually exists in `.claude/agents/` and `.claude/skills/`

### 3. Audit Skills

For each skill:
- Verify referenced file paths still exist
- Check for missing cross-references (see cross-reference table in claude-architect agent)
- Flag skills that reference outdated patterns

### 4. Audit Agents

For each agent:
- Verify listed tech stack matches current project
- Check that referenced skills exist
- Verify tool list is appropriate for the agent's scope

### 5. Review Learnings

For each learning:
- Check if the problem still exists (was it fixed by refactoring?)
- Check promotion criteria (3+ occurrences, cross-service applicability)
- Flag stale entries for archival

### 6. Generate Report

Output a structured report:

```
## Claude Infrastructure Sync Report

### CLAUDE.md Files
- [file]: {status: OK | NEEDS UPDATE}
  - {issue description if not OK}

### Skills
- [skill]: {status: OK | NEEDS UPDATE}
  - {issue description if not OK}

### Agents
- [agent]: {status: OK | NEEDS UPDATE}
  - {issue description if not OK}

### Learnings
- {count} total, {count} promotion candidates, {count} stale

### Cross-References
- {any missing bidirectional references}

### Recommended Actions
1. {action item}
2. {action item}
```

### 7. Ask User

Present the report and ask: "Would you like me to apply the recommended updates?"

If yes, apply each fix, showing what changed.

## Important Rules

- NEVER auto-apply changes without showing the report first
- NEVER delete learnings without user confirmation
- ALWAYS verify file existence before flagging as missing
- Report should be concise — only flag actual issues, not everything that's fine
- Focus on actionable findings, not exhaustive listings
