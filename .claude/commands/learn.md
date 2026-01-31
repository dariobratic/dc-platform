# Capture Learning from Current Session

Review the current conversation to identify and capture a reusable pattern.

## Steps to Execute

### 1. Analyze the Session

Look through the conversation for:
- Problems that required multiple attempts to solve
- Unexpected behaviors discovered in dependencies
- Workarounds for tool/library limitations
- Non-obvious configuration requirements
- Debugging patterns that revealed root causes

If no significant learning is found, tell the user: "No non-trivial patterns identified in this session."

### 2. Extract the Pattern

For each identified pattern, determine:
- **Problem**: What was the symptom or challenge?
- **Root Cause**: What was actually going wrong?
- **Solution**: What fixed it?
- **Trigger**: When would someone encounter this again?

### 3. Check for Duplicates

Search `.claude/learnings/` for existing entries that cover the same pattern.
If a duplicate exists, update it rather than creating a new one.

### 4. Create the Learning Entry

Write to `.claude/learnings/YYYY-MM-DD-{slug}.md` using today's date:

```markdown
# Title: Short description of the pattern

## Problem
[Symptom, error message, or misleading behavior]

## Solution
[What fixed it — code, config, or approach]

## When to Use
[Trigger conditions for applying this pattern]

## Related
- Files: [relevant file paths]
- Skills: [related skill names]
- Services: [affected services]
```

### 5. Check Promotion

If this pattern:
- Has been seen before (check learnings for similar entries)
- Applies broadly (multiple services or common workflow)
- Is non-obvious (would take >15 min to figure out)

Then suggest promoting it to a skill section. Ask the user if they want to proceed.

### 6. Summary

Show:
```
Learning captured: .claude/learnings/{filename}
Pattern: {one-line summary}
Related skills: {list}
Promotion candidate: Yes/No
```

## Important Rules

- NEVER capture trivial fixes (typos, missing imports, obvious bugs)
- ALWAYS include concrete code/config in the solution
- ALWAYS check for duplicates before creating
- ALWAYS reference related skills and files
- Keep entries focused — one pattern per file
- Use the slug from the core issue, not the symptom
