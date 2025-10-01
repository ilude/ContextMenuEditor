# Semantic Versioning Guide

This project follows [Semantic Versioning 2.0.0](https://semver.org/) for automated version management.

## Version Format

**MAJOR.MINOR.PATCH** (e.g., `1.2.3`)

- **MAJOR**: Breaking changes (incompatible API changes)
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

## Automated Versioning

Versions are calculated automatically from git history using the [Git Semantic Version action](https://github.com/marketplace/actions/git-semantic-version).

### How It Works

1. **No manual version editing** - The version is derived from git history
2. **Commit messages control version type** - Include markers in your commits
3. **Tags trigger releases** - Pushing a tag creates a GitHub release
4. **Builds get version numbers** - Every build has a proper semantic version

## Controlling Version Increments

Use these markers in your commit messages to control version increments:

### MAJOR Version (Breaking Changes)
Include `(MAJOR)` in your commit message:
```
feat: Completely redesign registry access API (MAJOR)

This is a breaking change that requires users to update their
integration code.
```

**Result**: `v1.5.3` → `v2.0.0`

**Use for**:
- Breaking API changes
- Removing features
- Major architectural changes
- Incompatible changes

### MINOR Version (New Features)
Include `(MINOR)` in your commit message:
```
feat: Add support for custom context menu icons (MINOR)

Users can now specify custom icons for their context menu items.
```

**Result**: `v1.5.3` → `v1.6.0`

**Use for**:
- New features
- New functionality
- Enhancements (backward compatible)
- Deprecations

### PATCH Version (Bug Fixes)
No marker needed - this is the default:
```
fix: Correct registry path resolution for non-ASCII characters

Fixes issue where context menu items with non-ASCII characters
were not properly detected.
```

**Result**: `v1.5.3` → `v1.5.4`

**Use for**:
- Bug fixes
- Performance improvements
- Documentation updates
- Code refactoring (no functional changes)

## Release Workflow

### 1. Develop and Commit
Work on your feature branch and commit with appropriate markers:
```bash
git add .
git commit -m "feat: Add dark mode toggle button (MINOR)"
git push origin feature/dark-mode
```

### 2. Merge to Main
Merge your PR to main:
```bash
git checkout main
git pull origin main
git merge feature/dark-mode
git push origin main
```

The GitHub Actions workflow will:
- ✅ Calculate the semantic version automatically
- ✅ Build the application with the correct version number
- ✅ Upload artifacts (kept for 90 days)

### 3. Create a Release
When ready to release, create and push a tag:
```bash
git tag v1.6.0
git push origin v1.6.0
```

The workflow will:
- ✅ Build with version `1.6.0`
- ✅ Create a ZIP package: `ContextMenuEditor-v1.6.0-win-x64.zip`
- ✅ Create a GitHub release with release notes
- ✅ Attach the ZIP file to the release

## Version Examples

### Scenario 1: Bug Fix Release
```
Current version: v1.2.5
Commits since last tag:
- fix: Resolve checkbox update issue
- fix: Handle null registry values

Next version: v1.2.6 (PATCH increment)
```

### Scenario 2: New Feature Release
```
Current version: v1.2.6
Commits since last tag:
- feat: Add registry backup feature (MINOR)
- fix: Update theme colors
- docs: Update README

Next version: v1.3.0 (MINOR increment, patch resets to 0)
```

### Scenario 3: Breaking Change Release
```
Current version: v1.3.0
Commits since last tag:
- refactor: Redesign MVVM architecture (MAJOR)
- feat: Add new plugin system (MINOR)
- fix: Various bug fixes

Next version: v2.0.0 (MAJOR takes precedence)
```

## Best Practices

### Commit Message Format
Use conventional commits format:
```
<type>: <description> [(<marker>)]

[optional body]

[optional footer]
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

### Examples
```bash
# Patch (bug fix)
git commit -m "fix: Correct disabled button text visibility in dark mode"

# Minor (new feature)
git commit -m "feat: Add support for background context menus (MINOR)"

# Major (breaking change)
git commit -m "refactor: Replace registry service with new provider model (MAJOR)"
```

## Version in Code

The version is injected into the build automatically:

**In .csproj**:
```xml
<Version>1.0.0</Version>  <!-- Default, overridden by build -->
```

**During build**:
```bash
dotnet build /p:Version=1.6.0
```

The version appears in:
- ✅ Assembly metadata
- ✅ File properties (right-click EXE → Properties → Details)
- ✅ About dialog (if implemented)

## Checking Current Version

### From Git
```bash
# Latest tag
git describe --tags --abbrev=0

# Calculate next version (simulates workflow)
# Requires the git-semver tool or GitHub Actions
```

### From GitHub Actions
Check the "Calculate semantic version" step output in any workflow run.

### From Release
The latest release on GitHub shows the current published version.

## Troubleshooting

### Version Not Incrementing
**Problem**: Version stays the same between builds  
**Solution**: Ensure you're pushing commits, not just building locally

### Wrong Version Type
**Problem**: Expected MINOR but got PATCH  
**Solution**: Include `(MINOR)` marker in your commit message

### Version Skipped
**Problem**: Went from v1.0.0 to v1.0.2  
**Solution**: A commit was tagged directly without a release build

## References

- [Semantic Versioning 2.0.0](https://semver.org/)
- [Git Semantic Version Action](https://github.com/marketplace/actions/git-semantic-version)
- [Conventional Commits](https://www.conventionalcommits.org/)
