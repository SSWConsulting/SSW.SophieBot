# Branching Strategy

To avoid massive stale branches, 
- If you have access to Settings, ensure "Automatically delete head branches" is set to true
- During development
 1. Check stale branches in your local regularly by running `git remote show origin`
 2. Clear local stale branches by running `git remote prune origin`