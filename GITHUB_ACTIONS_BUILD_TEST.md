# GitHub Actions Integration Guide: Build and Test for TQVaultAE Projects

This document provides comprehensive steps for integrating GitHub Actions to build and test the TQVaultAE .NET projects.

## Build Strategy

| Workflow | Trigger | Configurations | Retention | Purpose |
|-----------|----------|---------------|-----------|---------|
| `auto-version.yml` | `master` push, `version-info.json` changed | Permanent (GitHub Releases) | Production release |
| `build-and-test.yml` | `master` push, `src/**` changed | 3 days (artifacts) | Code verification |
| `build-and-test.yml` | Pull request to `master`, `src/**` changed | 3 days (artifacts) | PR verification |

### Key Principles
- **Two separate workflows**: One for releases, one for code verification
- **Version triggers releases**: Editing `version-info.json` triggers `auto-version.yml` which creates a release
- **Code changes trigger verification**: Changes to `src/**` trigger `build-and-test.yml` for PRs and merges
- **Release builds only**: Debug builds for local development
- **No infinite loops**: Version sync commit uses `[skip ci]` to prevent re-triggering

## Prerequisites

- Repository: https://github.com/EtienneLamoureux/TQVaultAE
- GitHub repository with Actions enabled
- Windows runner support (required for .NET 10.0 Windows)
- **Personal Access Token (PAT)**: Required to bypass branch protection rules when pushing version changes (see setup below)

### Setting Up the Version Bump Token (PAT)

The workflow requires a Personal Access Token to push commits and create tags on protected branches. The default `GITHUB_TOKEN` cannot bypass branch protection rules.

**Steps to create the PAT:**

1. Go to your GitHub account → **Settings** → **Developer settings** → **Personal access tokens** → **Tokens (classic)**
2. Click **Generate new token (classic)**
3. Give it a name like "TQVaultAE Version Bump"
4. Select scopes:
   - ✅ `repo` - Full control of private repositories
   - ✅ `workflow` - Update GitHub Action workflows
5. Click **Generate token**
6. **Copy the token immediately** (you won't see it again)

**Add the token to your repository:**

1. Go to your repository on GitHub → **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Name: `VERSION_BUMP_TOKEN`
4. Value: Paste your PAT from step 6
5. Click **Add secret**

**Configure branch protection to allow the PAT:**

1. Go to **Settings** → **Branches**
2. Click **Edit** on your protection rule for `master`
3. Under "Allow specified actors to bypass required pull requests", add the GitHub user account associated with the PAT
4. Alternatively, enable **Allow force pushes** and **Allow deletions** for admin users
5. Click **Save changes**

**Note**: If using an organization account, the PAT owner must have appropriate permissions in the organization.

## Projects to Build

### Executables

| Project | Target Framework | Output Type | Path |
|---------|-----------------|-------------|------|
| TQVaultAE.GUI | .NET 10.0 Windows | WinExe | src/TQVaultAE.GUI/TQVaultAE.GUI.csproj |
| ARZExplorer | .NET 10.0 Windows | WinExe | src/ARZExplorer/ArzExplorer.csproj |
| TQ.SaveFilesExplorer | .NET Framework 4.8 | WinExe | src/TQSaveFilesExplorer/TQ.SaveFilesExplorer.csproj |

### Libraries

| Project | Target Framework | Path |
|---------|-----------------|------|
| TQVaultAE.Domain | .NET 10.0 | src/TQVaultAE.Domain/TQVaultAE.Domain.csproj |
| TQVaultAE.Application | .NET 10.0 | src/TQVaultAE.Application/TQVaultAE.Application.csproj |
| TQVaultAE.Services | .NET 10.0 | src/TQVaultAE.Services/TQVaultAE.Services.csproj |
| TQVaultAE.Presentation | .NET 10.0 | src/TQVaultAE.Presentation/TQVaultAE.Presentation.csproj |
| TQVaultAE.Data | .NET 10.0 | src/TQVaultAE.Data/TQVaultAE.Data.csproj |
| TQVaultAE.Config | .NET 10.0 | src/TQVaultAE.Config/TQVaultAE.Config.csproj |
| TQVaultAE.Logs | .NET 10.0 | src/TQVaultAE.Logs/TQVaultAE.Logs.csproj |
| TQVaultAE.Services.Win32 | .NET 10.0 Windows | src/TQVaultAE.Services.Win32/TQVaultAE.Services.Win32.csproj |

### Test Projects

| Project | Path |
|---------|------|
| TQVaultAE.Tests | src/TQVaultAE.Tests/TQVaultAE.Tests.csproj |

## Why This Build Strategy?

For detailed information about version management and consistency, see [VERSIONING.md](VERSIONING.md).

### Main Branch (Production Releases)
- **Release builds only** - optimized for production
- **GitHub Releases** - permanent storage for version history
- **Published artifacts** available for users indefinitely

### Pull Requests (Verification)
- **Release builds only** - verify code compiles and tests pass
- **3-day retention** - sufficient for PR review and quick feedback
- **Faster feedback loop** for contributors

### Feature Branches
- **Same as PR** - Release builds, 3-day retention
- **Local debugging** - developers build Debug configuration locally

## Workflow Files Overview

This repository already includes the necessary GitHub Actions workflow files:

```
.github/
  workflows/
    auto-version.yml          # Handles version sync and releases
    build-and-test.yml        # Handles code verification
  scripts/
    Update-Version.ps1        # PowerShell script for version management
```

**Workflow Strategy:**
- **auto-version.yml**: Triggers when `version-info.json` is pushed to `master`. Syncs version, builds, creates tags, and publishes releases with ZIP archives.
- **build-and-test.yml**: Triggers on `master` pushes (with `src/**` changes) and PRs to `master`. Builds and tests for verification with 3-day artifact retention.

**Important**: Both workflows use a single solution file:
- `TQVaultAE.slnx` - Modern .NET 10.0 SDK-style projects (all projects including .NET Framework 4.8 via TQ.SaveFilesExplorer)

The Magick.NET-Q8-AnyCPU NuGet package includes all required native ImageMagick binaries, so no separate installation is needed.

#### Trigger Options

The workflows can be triggered by:

**auto-version.yml:**
- **Push events**: Runs when `version-info.json` is pushed to `master` branch

**build-and-test.yml:**
- **Push events**: Runs when code in `src/**` is pushed to `master` branch
- **Pull requests**: Runs when PRs with `src/**` changes are created/updated against `master`
- **Workflow dispatch**: Manual trigger from GitHub Actions tab
- **Scheduled runs**: Add `schedule` trigger for periodic builds

Example of scheduled builds for `build-and-test.yml`:

```yaml
on:
  schedule:
    - cron: '0 0 * * 0'  # Weekly on Sunday at midnight
  push:
    branches: [ master ]
    paths:
      - 'src/**'
```

#### Runner Options

- `windows-latest`: Current Windows runner (recommended for .NET 10.0 Windows)
- `windows-2022`: Specific Windows Server 2022 runner

#### dotnet CLI Configuration Options

| Option | Description | Example |
|--------|-------------|---------|
| `--configuration` | Build configuration (Release for distribution) | `--configuration=Release` |
| `--no-restore` | Skip restore step (use with restore first) | `--no-restore` |
| `--no-build` | Skip build step (use with build first) | `--no-build` |
| `--verbosity` | Output verbosity | `--verbosity=minimal`, `normal`, `detailed` |

**Example build command:**
```bash
dotnet restore TQVaultAE.slnx
dotnet build TQVaultAE.slnx --configuration Release --no-restore
```

**Note**: For local Debug builds, use `--configuration=Debug`

#### Test Options

```yaml
# Run all tests (Release configuration for production CI/CD)
dotnet test TQVaultAE.slnx --configuration Release

# Run specific test class
dotnet test TQVaultAE.slnx --filter "FullyQualifiedName~GameFileServiceTests"

# Run specific test
dotnet test TQVaultAE.slnx --filter "FullyQualifiedName~GameFileServiceTests.GetGamePath_ReturnsExpected"

# Run tests with coverage
dotnet test TQVaultAE.slnx --configuration Release --collect:"XPlat Code Coverage;Format=opencover"

# Note: Use Debug configuration locally for debugging with full symbols
dotnet test TQVaultAE.slnx --configuration Debug
```

## Advanced Workflow Features

#### Conditional Steps

```yaml
- name: Upload artifacts
  if: success()
  uses: actions/upload-artifact@v4
  with:
    name: build-output
    path: bin/Release/
```

#### Matrix Builds for Multiple Platforms

```yaml
strategy:
  matrix:
    configuration: [Debug, Release]
    
runs-on: windows-latest

steps:
  - name: Build Release
    run: dotnet build TQVaultAE.slnx --configuration ${{ matrix.configuration }}
    if: github.ref == 'refs/heads/main'
```

**Note**: Matrix builds multiply execution time. Use only when testing multiple configurations is necessary.

#### Caching Dependencies

```yaml
- name: Cache NuGet packages
  uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

## Adding Status Badges

Add these markdown badges to your README.md to display workflow status:

```markdown
![Build and Test](https://github.com/EtienneLamoureux/TQVaultAE/workflows/Build%20and%20Test/badge.svg)
![Auto Version and Release](https://github.com/EtienneLamoureux/TQVaultAE/workflows/Auto%20Version%20and%20Release/badge.svg)
```



## Testing from a Fork

If you're testing these workflows from a fork (e.g., `https://github.com/hguy/TQVaultAE.git`), there are several important considerations:

### Fork-Specific Configuration

#### 1. Enable GitHub Actions on Your Fork
1. Go to your fork on GitHub
2. Navigate to **Settings** → **Actions** → **General**
3. Under "Actions permissions", select:
   - **Allow all actions and reusable workflows** (or at least "Allow local Actions only")
4. Check **Allow GitHub Actions to create and approve pull requests** (optional, for automated PRs)
5. Click **Save**

#### 2. Configure Workflow Permissions
The workflows already include explicit permissions, but you should verify:

```yaml
permissions:
  contents: write    # Required for pushing tags and commits
  actions: read      # Required for reading workflow artifacts
  checks: write      # Required for publishing test results
```

#### 3. Important Fork Considerations

**Version Bump on Master Branch:**
- On your fork, when you push `version-info.json` to `master`, `auto-version.yml` will:
  1. Read version from `version-info.json`
  2. Sync version to all project files
  3. Create a git tag (e.g., `4.5.0`)
  4. Push the changes back to your fork
  5. Create a GitHub Release on your fork with ZIP archives

**This means:**
- Your fork will have its own version numbers (independent of the upstream repo)
- Tags will be created on your fork's `master` branch
- GitHub Releases will appear on your fork, not the upstream repo

#### 4. Testing Pull Requests from Your Fork

When you create a PR from your fork to the upstream repo:
- The `build-and-test.yml` workflow runs in the **upstream repository context**
- It uses the upstream repo's secrets and permissions
- The upstream maintainer controls whether Actions run on PRs from forks

**To test PR workflows on your fork:**
1. Create a feature branch on your fork: `git checkout -b feature/my-change`
2. Push to your fork: `git push origin feature/my-change`
3. Create a PR **within your fork** (fork's master ← fork's feature branch)
4. The `build-and-test.yml` workflow will run using your fork's settings
- No version changes or releases are created (only build/test verification)

#### 5. Preventing Unintended Releases on Forks

If you want to test the workflow without creating actual releases on your fork, you can temporarily modify `auto-version.yml` conditions:

```yaml
# Original (creates releases on master)
on:
  push:
    branches: [master]
    paths:
      - 'version-info.json'

# Modified for testing (only run on upstream repo)
on:
  push:
    branches: [master]
    paths:
      - 'version-info.json'

jobs:
  version-and-release:
    if: github.repository == 'EtienneLamoureux/TQVaultAE'
    # ... rest of job
```



#### 3. Important Fork Considerations

**Version Increment on Master Branch:**
- On your fork, when you push `version-info.json` to `master`, `auto-version.yml` will:
  1. Read the version from `version-info.json`
  2. Sync version to all project files
  3. Create a git tag (e.g., `4.5.0`)
  4. Push the changes back to your fork
  5. Create a GitHub Release on your fork with ZIP archives

**This means:**
- Your fork will have its own version numbers (independent of the upstream repo)
- Tags will be created on your fork's `master` branch
- GitHub Releases will appear on your fork, not the upstream repo

#### 4. Testing Pull Requests from Your Fork

When you create a PR from your fork to the upstream repo:
- The `build-and-test.yml` workflow runs in the **upstream repository context**
- It uses the upstream repo's secrets and permissions
- The upstream maintainer controls whether Actions run on PRs from forks
- No version changes or releases are created (only build/test verification)

**To test PR workflows on your fork:**
1. Create a feature branch on your fork: `git checkout -b feature/my-change`
2. Push to your fork: `git push origin feature/my-change`
3. Create a PR **within your fork** (fork's master ← fork's feature branch)
4. The `build-and-test.yml` workflow will run using your fork's settings

#### 5. Preventing Unintended Releases on Forks

If you want to test the workflow without creating actual releases on your fork, you can temporarily modify the `auto-version.yml` conditions:

```yaml
# Original (creates releases on master)
on:
  push:
    branches: [master]
    paths:
      - 'version-info.json'

# Modified for testing (only run on upstream repo)
on:
  push:
    branches: [master]
    paths:
      - 'version-info.json'
```

To skip releases on forks entirely, add a job-level condition:

```yaml
jobs:
  version-and-release:
    if: github.repository == 'EtienneLamoureux/TQVaultAE'
    # ... rest of job
```

#### 6. Required Repository Settings

On your fork, ensure these settings are configured:

**Settings → Actions → General:**
- ✅ **Read and write permissions** (for workflows to push commits and tags)
- ✅ **Allow GitHub Actions to create and approve pull requests** (optional)

**Settings → Secrets and variables → Actions:**
- `VERSION_BUMP_TOKEN`: Your Personal Access Token (required to push to protected branches)

#### 7. Clean Up After Testing

If you've created test releases/tags on your fork:

```bash
# Delete local tags
git tag -d 4.5.0
git tag -d 4.6.0

# Delete remote tags on your fork
git push origin --delete 4.5.0
git push origin --delete 4.6.0

# Or delete all remote tags
# WARNING: Be careful with this!
git push origin --delete $(git tag -l)
```

**Delete GitHub Releases from your fork:**
1. Go to your fork on GitHub
2. Navigate to **Releases**
3. Click on each release → **Delete**

### Fork Testing Checklist

- [ ] GitHub Actions enabled on your fork
- [ ] Workflow permissions set to "Read and write"
- [ ] Tested on a feature branch first
- [ ] Understand that versions/tags will be created on your fork
- [ ] Know how to clean up test releases if needed



## Best Practices

1. **Use Windows runners**: Required for .NET 10.0 Windows projects
2. **Use .NET SDK commands**: Use `dotnet restore` and `dotnet build` for SDK-style projects
3. **Release builds only**: Debug builds for local development only
4. **Appropriate retention**: 3 days for PRs/features, permanent for main via GitHub Releases
5. **Cache dependencies**: Speed up builds with NuGet caching
6. **Run tests on every push**: Catch issues early
7. **Separate build and test jobs**: Run in parallel when possible
8. **Upload test results**: Debug test failures with artifacts
9. **Use workflow dispatch**: Enable manual triggering
10. **Set artifact retention**: Balance storage costs with debugging needs



## Workflow Summary

```
feature/* → PR → main
   ↓         ↓     ↓
  Verify   Verify  Release
 (3 days) (3 days) (permanent)
```

For detailed version information, see [VERSIONING.md](VERSIONING.md).

## Additional Resources

- GitHub Actions Documentation: https://docs.github.com/en/actions
- .NET SDK on GitHub Actions: https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net
- dotnet CLI Reference: https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build
- xUnit Documentation: https://xunit.net/docs/getting-started/netcore/cmdline
