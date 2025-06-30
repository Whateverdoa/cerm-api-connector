# Code Migration Guide

This guide will help you migrate the existing CERM API code from the current structure to the new repository structure.

## Step 1: Copy Files to New Structure

### Main Library Files
Copy from `ConsoleApp1_cermapi_module/cerm api module/` to `src/CermApiConnector/`:

```bash
# Configuration files
cp "ConsoleApp1_cermapi_module/cerm api module/Configuration/CermApiSettings.cs" "src/CermApiConnector/Configuration/"
cp "ConsoleApp1_cermapi_module/cerm api module/Configuration/CermApiPaths.cs" "src/CermApiConnector/Configuration/"

# Extensions
cp "ConsoleApp1_cermapi_module/cerm api module/Extensions/ServiceCollectionExtensions.cs" "src/CermApiConnector/Extensions/"

# Models
cp "ConsoleApp1_cermapi_module/cerm api module/Models/"*.cs "src/CermApiConnector/Models/"

# Services
cp "ConsoleApp1_cermapi_module/cerm api module/Services/CermApiClient.cs" "src/CermApiConnector/Services/"

# Helpers
cp "ConsoleApp1_cermapi_module/cerm api module/Helpers/UserSecretsHelper.cs" "src/CermApiConnector/Helpers/"
```

### Test Files
Copy from `CermApiModule.Tests/` to `tests/CermApiConnector.Tests/`:

```bash
# Test files
cp "CermApiModule.Tests/"*.cs "tests/CermApiConnector.Tests/"
cp "CermApiModule.Tests/appsettings.json" "tests/CermApiConnector.Tests/"

# Test data
cp "CermApiModule.Tests/F003ADB6G8.json" "tests/CermApiConnector.Tests/"
```

### Documentation
Copy documentation files to `docs/`:

```bash
# Documentation
cp "ConsoleApp1_cermapi_module/cerm api module/Documentation/"*.md "docs/"
```

## Step 2: Update Namespaces

Replace all occurrences of `aws_b2b_mod1` with `CermApiConnector` in all copied files:

### PowerShell (Windows)
```powershell
Get-ChildItem -Path "src/CermApiConnector" -Recurse -Include "*.cs" | ForEach-Object {
    (Get-Content $_.FullName) -replace 'aws_b2b_mod1', 'CermApiConnector' | Set-Content $_.FullName
}

Get-ChildItem -Path "tests/CermApiConnector.Tests" -Recurse -Include "*.cs" | ForEach-Object {
    (Get-Content $_.FullName) -replace 'aws_b2b_mod1', 'CermApiConnector' | Set-Content $_.FullName
}
```

### Bash (Linux/macOS)
```bash
find src/CermApiConnector -name "*.cs" -type f -exec sed -i 's/aws_b2b_mod1/CermApiConnector/g' {} +
find tests/CermApiConnector.Tests -name "*.cs" -type f -exec sed -i 's/aws_b2b_mod1/CermApiConnector/g' {} +
```

## Step 3: Update Project References

### In Test Files
Update the project reference in test files from:
```csharp
using aws_b2b_mod1.Services;
using aws_b2b_mod1.Models;
```

To:
```csharp
using CermApiConnector.Services;
using CermApiConnector.Models;
```

### In Test Project File
Update `tests/CermApiConnector.Tests/CermApiConnector.Tests.csproj`:
```xml
<ProjectReference Include="..\..\src\CermApiConnector\CermApiConnector.csproj" />
```

## Step 4: Clean Up Sensitive Data

### Remove Hardcoded Credentials
Search for and remove any hardcoded credentials in:
- Configuration files
- Test files
- Documentation

### Update Configuration Examples
Replace real values with placeholders in:
- `appsettings.json` files
- `.env.example` files
- Documentation examples

## Step 5: Verify Build

After migration, verify everything builds correctly:

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests (unit tests only, without credentials)
dotnet test --filter "Category!=Integration"
```

## Step 6: Update Documentation

### Update File Paths
Update any file paths in documentation to reflect the new structure.

### Update Examples
Ensure all code examples in documentation use the new namespace `CermApiConnector`.

## Step 7: Git Setup

### Initialize Git (if not already done)
```bash
git init
git remote add origin https://github.com/Whateverdoa/cerm-api-connector.git
```

### Add Files
```bash
git add .
git commit -m "Initial commit: CERM API Connector library"
git push -u origin main
```

## Step 8: Security Verification

### Check .gitignore
Ensure the following are in `.gitignore`:
```
# User secrets and environment files
.env
appsettings.Development.json
**/appsettings.local.json

# Build outputs
bin/
obj/
*.nupkg

# IDE files
.vs/
.vscode/
*.user
*.suo
```

### Verify No Secrets
Run a final check to ensure no secrets are committed:
```bash
git log --all --full-history -- "*.env"
git log --all --full-history -S "password" -S "secret" -S "key"
```

## Step 9: Package Testing

### Test NuGet Package Creation
```bash
dotnet pack src/CermApiConnector/CermApiConnector.csproj --configuration Release
```

### Test Sample Application
```bash
cd samples/CermApiConnector.Sample
dotnet run
```

## Troubleshooting

### Common Issues

1. **Namespace conflicts**: Ensure all `aws_b2b_mod1` references are updated
2. **Missing dependencies**: Check that all NuGet packages are properly referenced
3. **Path issues**: Verify all relative paths are correct for the new structure
4. **Test failures**: Ensure test credentials are properly configured via User Secrets

### Verification Checklist

- [ ] All files copied to correct locations
- [ ] All namespaces updated from `aws_b2b_mod1` to `CermApiConnector`
- [ ] Project references updated
- [ ] No hardcoded credentials in source code
- [ ] Solution builds successfully
- [ ] Unit tests pass
- [ ] Documentation updated with new paths and namespaces
- [ ] .gitignore properly configured
- [ ] NuGet package builds successfully
- [ ] Sample application runs without errors

## Next Steps

After successful migration:

1. Set up User Secrets for testing
2. Run integration tests to verify functionality
3. Create first release/tag
4. Publish to NuGet (if desired)
5. Update repository settings and add collaborators
6. Set up GitHub repository secrets for CI/CD
