# GitHub Actions Workflow Changes

## Summary
Updated the build-release.yml workflow to automatically increment the patch version on every commit and create GitHub releases immediately.

## Key Changes Made

### 1. **Enable Automatic Patch Increments**
- Changed `bump_each_commit: false` → `bump_each_commit: true`
- Changed `search_commit_body: false` → `search_commit_body: true`
- **Result**: Every commit to main now increments the patch version (e.g., v1.0.0 → v1.0.1)

### 2. **Simplified Workflow to Single Job**
- Removed the separate `release` job that waited for tag pushes
- Added release creation directly to the `build` job
- **Result**: One workflow run = build + release (faster, simpler)

### 3. **Automatic Release Creation**
- Added `Create GitHub Release` step that runs on every main branch push
- Uses `softprops/action-gh-release@v2` to create releases with tags
- Automatically generates release notes from commits
- **Result**: Every commit to main creates a new release

### 4. **Removed Tag-Based Triggers**
- Removed `tags: - 'v*.*.*'` from workflow triggers
- Removed job condition checking for tag refs
- **Result**: Cleaner workflow that doesn't depend on pre-existing tags

## How It Works Now

### Automatic Versioning
1. **Patch (default)**: Regular commits increment patch version
   ```
   git commit -m "Fix button alignment issue"
   → v1.0.0 → v1.0.1
   ```

2. **Minor**: Use `(MINOR)` in commit message for new features
   ```
   git commit -m "(MINOR) Add export to CSV feature"
   → v1.0.1 → v1.1.0
   ```

3. **Major**: Use `(MAJOR)` in commit message for breaking changes
   ```
   git commit -m "(MAJOR) Rewrite registry service API"
   → v1.1.0 → v2.0.0
   ```

### Workflow Execution
1. Developer pushes commit to main branch
2. GitHub Actions calculates semantic version based on git history
3. Builds and publishes the application with that version
4. Creates a ZIP package
5. Uploads artifact for download
6. **Creates a GitHub Release** with:
   - The calculated version tag (e.g., v1.0.2)
   - The ZIP file attached
   - Auto-generated release notes from commits
   - Custom release body with installation instructions

### Pull Requests
- PR builds still run to validate code
- No releases are created for PRs (only builds and artifacts)
- Release creation only happens on pushes to main

## Testing the Changes

To verify this works:

1. **Make a test commit** (any small change):
   ```powershell
   git add .
   git commit -m "Test automatic release workflow"
   git push origin main
   ```

2. **Watch GitHub Actions**:
   - Go to: https://github.com/ilude/ContextMenuEditor/actions
   - The workflow should complete successfully
   - A new release should appear at: https://github.com/ilude/ContextMenuEditor/releases

3. **Check the release**:
   - Should have a new version tag (e.g., v1.1.1 if current is v1.1.0)
   - Should include the ZIP file
   - Should have release notes generated from commits

## Expected Behavior

✅ **Every commit to main** → New patch version + new release  
✅ **Commits with (MINOR)** → New minor version + new release  
✅ **Commits with (MAJOR)** → New major version + new release  
✅ **Pull requests** → Build only (no release)  
✅ **Manual trigger** → Build + release with current version  

## Migration from Old Workflow

The old workflow required:
1. Build job runs → creates tag
2. Tag push triggers → separate release job
3. Release job builds again → creates release

**Problem**: The tag push from step 1 didn't trigger step 2 in the same workflow run.

**New approach**: Build and release in one job, creating the tag and release together.

## Benefits

1. **Simpler**: One job instead of two
2. **Faster**: No duplicate builds
3. **Automatic**: No manual tag creation needed
4. **Reliable**: Release creation happens immediately after build
5. **Traceable**: Every commit = one version = one release
