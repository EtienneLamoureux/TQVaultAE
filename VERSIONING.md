# Version Consistency in TQVaultAE Suite

## Overview

All executables and DLLs use the same **4-part version number**:

```
Major.Minor.Build.Revision
```

Example: `4.4.1.0`
- **Major** (4): Major version, breaking changes
- **Minor** (4): Minor version, new features (manually updated)
- **Build** (1): Build number (manually updated for patch/hotfix releases)
- **Revision** (0): Reserved (currently always 0)

## Why Consistent Versioning?

### Benefits
1. **Unified Release Management** - All tools released together
2. **Clear Version Tracking** - Single version for entire suite
3. **Easier Support** - Know exact version across all tools
4. **User Clarity** - No confusion about which tool uses which version
5. **Dependency Alignment** - Services library version matches executables

### Use Cases

**User Scenario:**
```
"I'm using TQVaultAE v4.4.0 and TQ.SaveFilesExplorer v3.2.1"
→ This is confusing and potentially incompatible
→ With consistent versioning: "I'm using TQVaultAE Suite v4.4.0"
```

**Developer Scenario:**
```
Bug report: "Save file fails in v4.4.0"
→ Developer knows exactly which codebase version
→ All tools tested with same version
→ Reproducible across entire suite
```

## Implementation

### Version Source

Single source of truth: `version-info.json`

```json
{
  "Major": 4,
  "Minor": 4,
  "Build": 1,
  "Revision": 0
}
```

### Update Script

`Update-Version.ps1` script ensures consistency:

1. Reads version from `version-info.json` (read-only, does not modify it)
2. Updates ALL SDK-style .csproj files with SAME version
3. Does NOT modify `version-info.json` - it's the source of truth

### GitHub Actions Workflows

Version management is handled by **two separate workflows**:

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| `auto-version.yml` | Push to `master` with `version-info.json` changes | Sync version, build, create tag and GitHub Release |
| `build-and-test.yml` | Push to `master` with `src/**` changes OR PR to `master` | Build and test for verification |

### Version Bump Workflow (`auto-version.yml`)

1. **Edit `version-info.json`**: Update Major/Minor/Build as needed
2. **Push to `master`**: Commit and push the change
3. **auto-version.yml triggers**: Detects `version-info.json` changed
4. **Sync version**: Script reads `version-info.json` and syncs to all project files
5. **Commit sync**: GitHub Actions commits synced files with `[skip ci]` to prevent infinite loops
6. **Build**: Compiles release artifacts
7. **Create tag**: Creates git tag (e.g., `4.4.1`)
8. **Create ZIP archives**: Packages each application as `NAME-VERSION.zip`
9. **Create release**: Publishes GitHub Release with ZIP archives

### No Infinite Loops

The workflow uses `[skip ci]` in the commit message to prevent GitHub Actions from re-triggering on the version sync commit:

```bash
git commit -m "chore: sync version from version-info.json [skip ci]"
```

### Version Increment Rules

| Event | Version Change | Workflow |
|-------|----------------|----------|
| Edit `version-info.json` and push to `master` | Manual (Major/Minor/Build in JSON) | auto-version.yml |
| Push to `master` with `src/**` changes | None (uses current version) | build-and-test.yml |
| PR to `master` with `src/**` changes | None (uses current version) | build-and-test.yml |

### Files Updated by Sync

Each sync modifies ALL these SDK-style project files:

```
src/TQVaultAE.GUI/TQVaultAE.GUI.csproj
src/TQSaveFilesExplorer/TQ.SaveFilesExplorer.csproj
src/ARZExplorer/ArzExplorer.csproj
src/TQVaultAE.Services.Win32/TQVaultAE.Services.Win32.csproj
src/TQVaultAE.Domain/TQVaultAE.Domain.csproj
src/TQVaultAE.Application/TQVaultAE.Application.csproj
src/TQVaultAE.Services/TQVaultAE.Services.csproj
src/TQVaultAE.Presentation/TQVaultAE.Presentation.csproj
src/TQVaultAE.Data/TQVaultAE.Data.csproj
src/TQVaultAE.Config/TQVaultAE.Config.csproj
src/TQVaultAE.Logs/TQVaultAE.Logs.csproj
```

All receive **identical version** from `version-info.json` on every sync.

**Note:** All projects now use SDK-style format with auto-generated assembly attributes. The old `Properties/AssemblyInfo.cs` files have been removed and their attributes moved into the `.csproj` files.

## Verification

### Before Build

```bash
# Check Version in all SDK-style projects
grep "<Version>" src/*/*.csproj
```

Expected output (all lines show same version):
```
# SDK-style csproj files (all projects)
src/TQVaultAE.GUI/TQVaultAE.GUI.csproj:    <Version>4.4.1.0</Version>
src/TQSaveFilesExplorer/TQ.SaveFilesExplorer.csproj:    <Version>4.4.1.0</Version>
src/ARZExplorer/ArzExplorer.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Services.Win32/TQVaultAE.Services.Win32.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Domain/TQVaultAE.Domain.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Application/TQVaultAE.Application.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Services/TQVaultAE.Services.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Presentation/TQVaultAE.Presentation.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Data/TQVaultAE.Data.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Config/TQVaultAE.Config.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Logs/TQVaultAE.Logs.csproj:    <Version>4.4.1.0</Version>
```

### After Build

```powershell
# Check compiled executables (.NET 10.0 Windows)
(Get-Item "src\TQVaultAE.GUI\bin\Release\net10.0-windows\TQVaultAE.exe").VersionInfo.FileVersion
(Get-Item "src\ARZExplorer\bin\Release\net10.0-windows\ARZExplorer.exe").VersionInfo.FileVersion

# Check compiled executables (.NET Framework 4.8)
(Get-Item "src\TQSaveFilesExplorer\bin\Release\net48\TQ.SaveFilesExplorer.exe").VersionInfo.FileVersion

# Check compiled DLLs (.NET 10.0)
(Get-Item "src\TQVaultAE.Domain\bin\Release\net10.0\TQVaultAE.Domain.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Application\bin\Release\net10.0\TQVaultAE.Application.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Services\bin\Release\net10.0\TQVaultAE.Services.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Presentation\bin\Release\net10.0\TQVaultAE.Presentation.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Data\bin\Release\net10.0\TQVaultAE.Data.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Config\bin\Release\net10.0\TQVaultAE.Config.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Logs\bin\Release\net10.0\TQVaultAE.Logs.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Services.Win32\bin\Release\net10.0-windows\TQVaultAE.Services.Win32.dll").VersionInfo.FileVersion
```

All should return: `4.4.1.0` (or current synchronized version)

### From Executable Properties

1. Right-click executable → Properties → Details tab
2. Check "File version" field
3. All executables and DLLs should show same version

## Troubleshooting

### Issue: Versions Don't Match

**Symptom:**
```
TQVaultAE.exe: 4.4.1.0
TQ.SaveFilesExplorer.exe: 4.4.0.0
```

**Solution:**
1. Run the version sync script to synchronize all files:
   ```bash
   pwsh -File .github/scripts/Update-Version.ps1 -VersionType "Sync"
   ```
2. Commit the changes:
   ```bash
   git add -u
   git commit -m "chore: sync version from version-info.json"
   git push
   ```
3. Verify all files now show same version:
   ```bash
   grep "<Version>" src/*/*.csproj
   ```

### Issue: Wildcard Version in Project Files

**Symptom:**
```xml
<Version>4.4.*</Version>
```

**Solution:**
The sync script replaces wildcards with exact version. After running Update-Version.ps1 with `-VersionType "Sync"`, all csproj files should have:

```xml
<Version>4.4.1.0</Version>
```

### Issue: Workflow Not Triggering on Version Bump

**Symptom:**
Editing `version-info.json` doesn't trigger `auto-version.yml`.

**Solution:**
1. Verify workflow file exists at `.github/workflows/auto-version.yml`
2. Check that `version-info.json` is in the repository root
3. Confirm the push is to `master` branch
4. Verify workflow has permissions: `contents: write`
5. Check GitHub Actions tab for any workflow errors

### Issue: Infinite Workflow Loop

**Symptom:**
Workflow keeps triggering itself repeatedly.

**Solution:**
The workflow commit uses `[skip ci]` to prevent this. Verify:
1. Commit message includes `[skip ci]`:
   ```bash
   git commit -m "chore: sync version from version-info.json [skip ci]"
   ```
2. If loops still occur, check that no other files are triggering the workflow

### Issue: GitHub Release Upload Fails

**Symptom:**
Release creation fails with 404 error on individual files.

**Solution:**
The workflow now creates ZIP archives before release. Verify:
1. `Compress-Archive` step runs successfully
2. ZIP files are created: `TQVaultAE.GUI-*.zip`, `TQSaveFilesExplorer-*.zip`, `ARZExplorer-*.zip`
3. Release step uses ZIP files, not individual DLLs

## Best Practices

1. **Never manually edit Version tags** - Use Update-Version.ps1 with `-VersionType "Sync"`
2. **Always verify consistency** - Check all executables before release
3. **Use version-info.json** - Single source of truth (read-only for the sync script)
4. **Test all executables** - Ensure they work at same version
5. **Document version changes** - Keep release notes for all tools
6. **Maintain synchronization** - All 10 project files must have identical versions
7. **Use `[skip ci]`** - The workflow automatically includes this in sync commits
8. **Two workflows**: Let `auto-version.yml` handle releases, `build-and-test.yml` handle PRs

## Next Release Preparation

When ready to release version 4.5.0.0:

```bash
# Step 1: Edit version-info.json with new version
# Edit version-info.json:
# {
#   "Major": 4,
#   "Minor": 5,
#   "Build": 0,
#   "Revision": 0
# }

# Step 2: Commit and push to master
git add version-info.json
git commit -m "chore: bump version to 4.5.0.0"
git push origin master

# Step 3: auto-version.yml workflow will automatically:
# - Sync version to all project files
# - Build release artifacts
# - Create git tag "4.5.0"
# - Create ZIP archives
# - Publish GitHub Release

# Verify the release:
# 1. Check GitHub → Releases → v4.5.0
# 2. Download ZIP archives
# 3. Verify executables show version 4.5.0.0
```

## Related Documentation

- [GitHub Actions Build and Test Guide](GITHUB_ACTIONS_BUILD_TEST.md) - Detailed CI/CD workflow documentation
- [Agent Guidelines](AGENTS.md) - Development guidelines for AI agents
- [Contributing Guide](CONTRIBUTING.md) - How to contribute to the project

## Workflow Files

The versioning system is implemented by these GitHub Actions workflows:

| File | Purpose |
|------|---------|
| `.github/workflows/auto-version.yml` | Handles version bumps and releases (triggered by `version-info.json` changes) |
| `.github/workflows/build-and-test.yml` | Handles build verification for PRs and code pushes |
| `.github/scripts/Update-Version.ps1` | PowerShell script that syncs version to all project files |
