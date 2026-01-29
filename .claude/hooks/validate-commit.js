#!/usr/bin/env node

/**
 * Validates commit messages follow Conventional Commits format.
 * Format: type(scope): description
 * 
 * Types: feat, fix, refactor, docs, test, chore
 * Scope: service name or 'platform'
 */

const fs = require('fs');

// Read stdin (hook input from Claude Code)
let input = '';
process.stdin.setEncoding('utf8');
process.stdin.on('data', chunk => input += chunk);
process.stdin.on('end', () => {
    try {
        const data = JSON.parse(input);
        
        // Only validate git commit commands
        if (data.tool_name !== 'Bash') {
            process.exit(0); // Allow non-bash tools
        }
        
        const command = data.tool_input?.command || '';
        
        // Check if this is a git commit
        if (!command.includes('git commit')) {
            process.exit(0); // Not a commit, allow
        }
        
        // Extract commit message from -m flag
        const messageMatch = command.match(/git commit[^"]*-m\s*"([^"]+)"/);
        if (!messageMatch) {
            // No -m flag or different format, allow (might be interactive)
            process.exit(0);
        }
        
        const commitMessage = messageMatch[1];
        
        // Validate Conventional Commits format
        const validTypes = ['feat', 'fix', 'refactor', 'docs', 'test', 'chore'];
        const pattern = /^(feat|fix|refactor|docs|test|chore)\([\w-]+\):\s*.+$/;
        
        if (!pattern.test(commitMessage)) {
            // Output error message to stderr
            console.error(`\n❌ Invalid commit message format: "${commitMessage}"`);
            console.error(`\nExpected format: type(scope): description`);
            console.error(`Types: ${validTypes.join(', ')}`);
            console.error(`Example: feat(directory): add user authentication`);
            console.error(`\n`);
            
            // Exit code 2 = block the tool execution
            process.exit(2);
        }
        
        // Valid commit message
        console.error(`✓ Commit message format valid`);
        process.exit(0);
        
    } catch (e) {
        // On error, allow (don't block on hook failure)
        console.error(`Hook error: ${e.message}`);
        process.exit(0);
    }
});
