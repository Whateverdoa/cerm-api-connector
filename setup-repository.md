# Repository Setup Guide

This guide will help you set up the CERM API Connector repository from scratch.

## Prerequisites

- .NET 8.0 SDK installed
- Git installed
- GitHub account
- CERM API credentials for testing

## Step 1: Repository Creation

1. **Create GitHub Repository**:
   - Go to GitHub and create a new repository named `cerm-api-connector`
   - Make it public
   - Initialize with README
   - Add .gitignore: VisualStudio
   - Add license: MIT

2. **Clone Repository**:
   ```bash
   git clone https://github.com/YOUR_USERNAME/cerm-api-connector.git
   cd cerm-api-connector
   ```

## Step 2: Create Directory Structure

```bash
# Create main directories
mkdir -p src/CermApiConnector/{Configuration,Extensions,Models,Services,Helpers}
mkdir -p tests/CermApiConnector.Tests
mkdir -p samples/CermApiConnector.Sample
mkdir -p docs/examples
mkdir -p .github/workflows

# Create subdirectories for organization
mkdir -p src/CermApiConnector/Models/Requests
mkdir -p src/CermApiConnector/Models/Responses
```

## Step 3: Copy Project Files

Copy the project files we created:

```bash
# Solution file
cp CermApiConnector.sln ./

# Main library project
cp src/CermApiConnector/CermApiConnector.csproj src/CermApiConnector/

# Test project
cp tests/CermApiConnector.Tests/CermApiConnector.Tests.csproj tests/CermApiConnector.Tests/

# Sample project
cp samples/CermApiConnector.Sample/CermApiConnector.Sample.csproj samples/CermApiConnector.Sample/
cp samples/CermApiConnector.Sample/Program.cs samples/CermApiConnector.Sample/
cp samples/CermApiConnector.Sample/appsettings.json samples/CermApiConnector.Sample/

# Documentation
cp README.md ./
cp CONTRIBUTING.md ./
cp CHANGELOG.md ./

# GitHub workflows
cp .github/workflows/build.yml .github/workflows/
cp .github/workflows/test.yml .github/workflows/

# Test configuration
cp tests/CermApiConnector.Tests/.env.example tests/CermApiConnector.Tests/
```

## Step 4: Migrate Existing Code

Now migrate your existing CERM API code:

### 4.1 Copy Source Files

```bash
# From your existing project, copy the main library files:
cp "path/to/ConsoleApp1_cermapi_module/cerm api module/Configuration/"*.cs src/CermApiConnector/Configuration/
cp "path/to/ConsoleApp1_cermapi_module/cerm api module/Extensions/"*.cs src/CermApiConnector/Extensions/
cp "path/to/ConsoleApp1_cermapi_module/cerm api module/Models/"*.cs src/CermApiConnector/Models/
cp "path/to/ConsoleApp1_cermapi_module/cerm api module/Services/"*.cs src/CermApiConnector/Services/
cp "path/to/ConsoleApp1_cermapi_module/cerm api module/Helpers/"*.cs src/CermApiConnector/Helpers/
```

### 4.2 Copy Test Files

```bash
# Copy test files
cp "path/to/CermApiModule.Tests/"*.cs tests/CermApiConnector.Tests/
cp "path/to/CermApiModule.Tests/appsettings.json" tests/CermApiConnector.Tests/
cp "path/to/CermApiModule.Tests/F003ADB6G8.json" tests/CermApiConnector.Tests/
```

### 4.3 Copy Documentation

```bash
# Copy documentation
cp "path/to/ConsoleApp1_cermapi_module/cerm api module/Documentation/"*.md docs/
```

## Step 5: Update Namespaces

Update all namespace references from `aws_b2b_mod1` to `CermApiConnector`:

### Windows (PowerShell)
```powershell
# Update source files
Get-ChildItem -Path "src" -Recurse -Include "*.cs" | ForEach-Object {
    (Get-Content $_.FullName) -replace 'aws_b2b_mod1', 'CermApiConnector' | Set-Content $_.FullName
}

# Update test files
Get-ChildItem -Path "tests" -Recurse -Include "*.cs" | ForEach-Object {
    (Get-Content $_.FullName) -replace 'aws_b2b_mod1', 'CermApiConnector' | Set-Content $_.FullName
}
```

### Linux/macOS (Bash)
```bash
# Update source files
find src -name "*.cs" -type f -exec sed -i 's/aws_b2b_mod1/CermApiConnector/g' {} +

# Update test files
find tests -name "*.cs" -type f -exec sed -i 's/aws_b2b_mod1/CermApiConnector/g' {} +
```

## Step 6: Clean Up Sensitive Data

1. **Remove hardcoded credentials** from all files
2. **Update configuration examples** with placeholder values
3. **Verify .gitignore** includes sensitive files:

```gitignore
# Add to .gitignore if not already present
.env
appsettings.Development.json
**/appsettings.local.json
UserSecrets/
```

## Step 7: Build and Test

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run unit tests (without integration tests that need credentials)
dotnet test --filter "Category!=Integration"
```

## Step 8: Set Up User Secrets

For testing, set up user secrets:

```bash
# For test project
cd tests/CermApiConnector.Tests
dotnet user-secrets set "CermApiSettings:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "CermApiSettings:ClientSecret" "YOUR_CLIENT_SECRET"
dotnet user-secrets set "CermApiSettings:Username" "YOUR_USERNAME"
dotnet user-secrets set "CermApiSettings:Password" "YOUR_PASSWORD"

# For sample project
cd ../../samples/CermApiConnector.Sample
dotnet user-secrets set "CermApiSettings:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "CermApiSettings:ClientSecret" "YOUR_CLIENT_SECRET"
dotnet user-secrets set "CermApiSettings:Username" "YOUR_USERNAME"
dotnet user-secrets set "CermApiSettings:Password" "YOUR_PASSWORD"
```

## Step 9: Test Integration

Run integration tests to verify everything works:

```bash
cd tests/CermApiConnector.Tests
dotnet test --filter "Category=Integration"
```

## Step 10: Commit and Push

```bash
# Add all files
git add .

# Commit
git commit -m "Initial commit: CERM API Connector library

- Complete library structure with proper namespacing
- Comprehensive test suite with 28+ tests
- Sample application demonstrating usage
- Full documentation and contribution guidelines
- CI/CD workflows for automated testing
- NuGet package configuration"

# Push to GitHub
git push origin main
```

## Step 11: Repository Configuration

### 11.1 Set Up GitHub Secrets

For CI/CD workflows, add these secrets to your GitHub repository:

1. Go to repository Settings → Secrets and variables → Actions
2. Add the following secrets:
   - `CERM_CLIENT_ID`
   - `CERM_CLIENT_SECRET`
   - `CERM_USERNAME`
   - `CERM_PASSWORD`

### 11.2 Configure Branch Protection

1. Go to Settings → Branches
2. Add rule for `main` branch:
   - Require pull request reviews
   - Require status checks to pass
   - Include administrators

### 11.3 Set Up Environments

1. Go to Settings → Environments
2. Create environments:
   - `Test` (for test environment)
   - `Production` (for production environment)
3. Add environment-specific secrets

## Step 12: Create First Release

1. **Tag the release**:
   ```bash
   git tag -a v1.0.0 -m "Initial release of CERM API Connector"
   git push origin v1.0.0
   ```

2. **Create GitHub Release**:
   - Go to Releases → Create a new release
   - Choose tag v1.0.0
   - Add release notes from CHANGELOG.md
   - Attach NuGet package if desired

## Step 13: Optional - Publish to NuGet

If you want to publish to NuGet:

```bash
# Build package
dotnet pack src/CermApiConnector/CermApiConnector.csproj --configuration Release

# Publish (requires NuGet API key)
dotnet nuget push src/CermApiConnector/bin/Release/CermApiConnector.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Verification Checklist

- [ ] Repository created and cloned
- [ ] Directory structure created
- [ ] All source files copied and namespaces updated
- [ ] All test files copied and updated
- [ ] Documentation copied and updated
- [ ] No sensitive data in repository
- [ ] Solution builds successfully
- [ ] Unit tests pass
- [ ] User secrets configured for testing
- [ ] Integration tests pass
- [ ] GitHub workflows configured
- [ ] Repository settings configured
- [ ] First release created

## Troubleshooting

### Common Issues

1. **Build errors**: Check that all project references are correct
2. **Namespace errors**: Ensure all `aws_b2b_mod1` references are updated
3. **Test failures**: Verify user secrets are properly configured
4. **Missing files**: Check that all necessary files were copied

### Getting Help

- Check the [CONTRIBUTING.md](CONTRIBUTING.md) guide
- Review the [documentation](docs/)
- Create an issue on GitHub
- Check existing issues for similar problems

Congratulations! Your CERM API Connector repository is now set up and ready for development and collaboration.
