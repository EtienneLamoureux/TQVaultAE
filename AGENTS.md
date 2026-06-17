# TQVaultAE Agent Guidelines

## Build Commands

```bash
# Build solution (Any CPU)
dotnet build TQVaultAE.slnx
dotnet build TQVaultAE.slnx --configuration Release

# Build specific project
dotnet build src/TQVaultAE.GUI/TQVaultAE.GUI.csproj

# Restore packages
dotnet restore
```

**Solution File:** `TQVaultAE.slnx` (modern .NET 10.0 SDK-style format)

## Test Commands

```bash
# Run all tests
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj

# Run single test class
dotnet test --filter "FullyQualifiedName~TagServiceTests"

# Run single test method
dotnet test --filter "FullyQualifiedName~TagServiceTests.AddTag_WithNewTagName_ReturnsTrue"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage;Format=opencover"

# Build before testing
dotnet build src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --configuration Debug
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --no-build
```

## Code Style

### Formatting
- **Indentation**: Tabs
- **Braces**: K&R style (opening brace on same line)
- **Namespaces**: File-scoped
- **This Prefix**: Use `this.` for all instance members

### Naming Conventions
| Element | Convention | Example |
|---------|------------|---------|
| Classes/Interfaces | PascalCase | `ItemService`, `IItemService` |
| Private fields | camelCase with underscore | `_itemCount` |
| Interfaces | Prefix with `I` | `IFileIO` |

### Types & Nullable
- Enable nullable reference types: `<Nullable>enable</Nullable>`
- Use `?` for nullable parameters
- Initialize collections as empty rather than null

### Async Patterns
- Use `async`/`await` for I/O operations
- Name async methods with `Async` suffix: `LoadPlayerAsync()`
- Do not block on async code (avoid `.Result`, `.Wait()`)

### Error Handling
- Prefer Result types over exceptions for expected failures
- Log exceptions with context: `Log.LogError(ex, "Failed to load {Name}", name)`

## Testing

### Framework
- **xUnit** for tests, **AwesomeAssertions** for assertions, **Moq** for mocking

### What is a Meaningful Test? ⚠️ IMPORTANT

**AVOID trivial tests that add NO value:**
- ❌ Property getter/setter without logic (`*_CanBeSet`)
- ❌ Default value tests without behavior
- ❌ Constructor verification (`new Class()`)
- ❌ `x.Should().Be(y)` with constant values

**WRITE meaningful tests that verify:**
- ✅ Business logic and calculations
- ✅ Error handling and edge cases
- ✅ Interactions between components
- ✅ State transitions and side effects

**Rule:** If you can't answer "What bug would this test catch?" → DELETE it

### Cross-Platform Testing
- Use `Path.GetTempPath()` for paths
- Mock `IPathIO` for path control
- DirectoryNotFoundException handling differs on Linux

## Architecture

### Layer Dependencies
```
GUI → Presentation → Services → Domain → Data
     ↓
Application (DTOs, Results, Search)
```

### Projects
| Project | Framework | Purpose |
|---------|-----------|---------|
| TQVaultAE.GUI | .NET 10.0 | Windows Forms UI |
| TQVaultAE.Services | .NET 10.0 | Business logic |
| TQVaultAE.Application | .NET 10.0 | DTOs, Results |
| TQVaultAE.Data | .NET 10.0 | Data access |
| TQVaultAE.Domain | .NET 10.0 | Entities |
| TQVaultAE.Tests | .NET 10.0 | Unit tests |

### Key Abstractions (for DI)
- `IFileIO` - File operations
- `IPathIO` - Path operations
- `IGamePathService` - Game path resolution
- Register services in `src/TQVaultAE.GUI/Program.cs`

## Adding New Code

### Adding a Service
1. Create interface in `src/TQVaultAE.Application/contracts/Services/`
2. Implement in `src/TQVaultAE.Services/`
3. Use `IFileIO`/`IPathIO` for file operations
4. Register in `Program.cs`
5. Add tests with meaningful assertions

### Adding a Test
1. Create test class: `src/TQVaultAE.Tests/<Category>/<Name>Tests.cs`
2. Mock all dependencies in constructor
3. Follow Arrange-Act-Assert pattern
4. Verify actual behavior, not property assignments

## Resources
- GitHub: https://github.com/EtienneLamoureux/TQVaultAE
- Issues: https://github.com/EtienneLamoureux/TQVaultAE/issues
