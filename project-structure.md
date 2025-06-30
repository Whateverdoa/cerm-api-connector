# CERM API Connector - Project Structure

## Recommended Directory Structure

```
cerm-api-connector/
├── src/
│   └── CermApiConnector/
│       ├── CermApiConnector.csproj
│       ├── Configuration/
│       │   ├── CermApiSettings.cs
│       │   └── CermApiPaths.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       ├── Models/
│       │   ├── TokenResponse.cs
│       │   ├── AddressIdResponse.cs
│       │   ├── AddressDetailsResponse.cs
│       │   ├── AddressValidationResult.cs
│       │   ├── CreateAddressRequest.cs
│       │   ├── QuoteIdResponse.cs
│       │   ├── ProductIdResponse.cs
│       │   ├── SalesOrderIdResponse.cs
│       │   └── CermIdentifier.cs
│       ├── Services/
│       │   └── CermApiClient.cs
│       └── Helpers/
│           └── UserSecretsHelper.cs
├── tests/
│   └── CermApiConnector.Tests/
│       ├── CermApiConnector.Tests.csproj
│       ├── TestBase.cs
│       ├── TestData.cs
│       ├── AddressManagementTests.cs
│       ├── AuthenticationTests.cs
│       ├── CalculationTests.cs
│       ├── ProductTests.cs
│       ├── SalesOrderTests.cs
│       ├── IntegrationTests.cs
│       ├── appsettings.json
│       └── .env.example
├── docs/
│   ├── README.md
│   ├── CERM_API_Manual.md
│   ├── CERM_API_Testing_Guide.md
│   ├── CERM_API_Testing_Plan.md
│   └── examples/
│       ├── BasicUsage.md
│       ├── AddressManagement.md
│       ├── ProductCreation.md
│       └── IntegrationExample.md
├── samples/
│   └── CermApiConnector.Sample/
│       ├── CermApiConnector.Sample.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       └── .env.example
├── .github/
│   └── workflows/
│       ├── build.yml
│       └── test.yml
├── .gitignore
├── LICENSE
├── README.md
├── CONTRIBUTING.md
├── CHANGELOG.md
└── CermApiConnector.sln
```

## File Migration Plan

### From Current Structure:
```
ConsoleApp1_cermapi_module/cerm api module/
├── Configuration/
├── Extensions/
├── Models/
├── Services/
├── Tests/
├── Documentation/
└── Helpers/
```

### To New Structure:
1. **Main Library**: Move to `src/CermApiConnector/`
2. **Tests**: Move `CermApiModule.Tests/` to `tests/CermApiConnector.Tests/`
3. **Documentation**: Move to `docs/`
4. **Create Sample Project**: New `samples/CermApiConnector.Sample/`

## Steps to Reorganize

1. Create the new directory structure
2. Move and rename files appropriately
3. Update namespaces from `aws_b2b_mod1` to `CermApiConnector`
4. Update project references and dependencies
5. Create solution file
6. Update documentation paths and references
7. Remove sensitive data and create example files
8. Set up proper NuGet package configuration

## Namespace Changes

**From**: `aws_b2b_mod1.*`
**To**: `CermApiConnector.*`

Examples:
- `aws_b2b_mod1.Configuration` → `CermApiConnector.Configuration`
- `aws_b2b_mod1.Services` → `CermApiConnector.Services`
- `aws_b2b_mod1.Models` → `CermApiConnector.Models`
- `aws_b2b_mod1.Extensions` → `CermApiConnector.Extensions`

## Project File Updates

### Main Library (CermApiConnector.csproj)
- Target Framework: net6.0 or net8.0 (for broader compatibility)
- Package metadata for NuGet
- Proper dependency versions
- XML documentation generation

### Test Project (CermApiConnector.Tests.csproj)
- Reference main library project
- Test framework dependencies
- Test configuration files

### Sample Project (CermApiConnector.Sample.csproj)
- Console application demonstrating usage
- Reference main library
- Example configuration
