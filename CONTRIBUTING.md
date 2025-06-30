# Contributing to CERM API Connector

Thank you for your interest in contributing to the CERM API Connector! This document provides guidelines and information for contributors.

## 🤝 Code of Conduct

By participating in this project, you agree to abide by our Code of Conduct. Please be respectful and constructive in all interactions.

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Git
- A code editor (Visual Studio, VS Code, or JetBrains Rider recommended)
- Valid CERM API credentials for testing

### Development Setup

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/cerm-api-connector.git
   cd cerm-api-connector
   ```

3. **Set up the development environment**:
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Configure test credentials** (use User Secrets):
   ```bash
   dotnet user-secrets set "CermApiSettings:ClientId" "YOUR_TEST_CLIENT_ID" --project tests/CermApiConnector.Tests
   dotnet user-secrets set "CermApiSettings:ClientSecret" "YOUR_TEST_CLIENT_SECRET" --project tests/CermApiConnector.Tests
   dotnet user-secrets set "CermApiSettings:Username" "YOUR_TEST_USERNAME" --project tests/CermApiConnector.Tests
   dotnet user-secrets set "CermApiSettings:Password" "YOUR_TEST_PASSWORD" --project tests/CermApiConnector.Tests
   ```

5. **Run tests** to verify setup:
   ```bash
   dotnet test
   ```

## 📝 How to Contribute

### Reporting Issues

Before creating an issue, please:

1. **Search existing issues** to avoid duplicates
2. **Use the issue templates** when available
3. **Provide detailed information**:
   - Clear description of the problem
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment details (.NET version, OS, etc.)
   - Relevant logs or error messages

### Suggesting Features

For feature requests:

1. **Check existing feature requests** first
2. **Describe the use case** clearly
3. **Explain the benefits** to users
4. **Consider backwards compatibility**
5. **Provide examples** if possible

### Submitting Changes

#### Branch Naming Convention

- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `docs/description` - Documentation updates
- `refactor/description` - Code refactoring
- `test/description` - Test improvements

#### Pull Request Process

1. **Create a feature branch** from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes** following our coding standards

3. **Add or update tests** for your changes

4. **Update documentation** if needed

5. **Ensure all tests pass**:
   ```bash
   dotnet test
   ```

6. **Commit your changes** with clear messages:
   ```bash
   git commit -m "Add feature: description of what you added"
   ```

7. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

8. **Create a Pull Request** on GitHub

#### Pull Request Guidelines

- **Use descriptive titles** and descriptions
- **Reference related issues** using `#issue-number`
- **Keep changes focused** - one feature/fix per PR
- **Include tests** for new functionality
- **Update documentation** as needed
- **Ensure CI passes** before requesting review

## 🎯 Coding Standards

### C# Style Guidelines

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- Use **PascalCase** for public members
- Use **camelCase** for private fields and local variables
- Use **meaningful names** for variables and methods
- Add **XML documentation** for public APIs

### Code Quality

- **Write unit tests** for new functionality
- **Maintain test coverage** above 80%
- **Use async/await** for I/O operations
- **Handle exceptions** appropriately
- **Log important operations** using ILogger
- **Follow SOLID principles**

### Example Code Style

```csharp
/// <summary>
/// Creates a new address in the CERM API
/// </summary>
/// <param name="request">The address creation request</param>
/// <returns>Response containing the new address ID</returns>
public async Task<AddressIdResponse> CreateAddressAsync(CreateAddressRequest request)
{
    if (request == null)
        throw new ArgumentNullException(nameof(request));

    _logger.LogInformation("Creating address for customer {CustomerId}", request.CustomerId);

    try
    {
        var response = await SendRequestAsync(request);
        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create address for customer {CustomerId}", request.CustomerId);
        throw;
    }
}
```

## 🧪 Testing Guidelines

### Test Structure

- **Arrange**: Set up test data and dependencies
- **Act**: Execute the method being tested
- **Assert**: Verify the results

### Test Naming

Use descriptive test names following the pattern:
`MethodName_Scenario_ExpectedResult`

Example: `CreateAddressAsync_WithValidRequest_ReturnsSuccessResponse`

### Test Categories

- **Unit Tests**: Test individual methods in isolation
- **Integration Tests**: Test API interactions with real endpoints
- **Performance Tests**: Verify response times and throughput

### Example Test

```csharp
[Fact]
public async Task CreateAddressAsync_WithValidRequest_ReturnsSuccessResponse()
{
    // Arrange
    var request = new CreateAddressRequest
    {
        CustomerId = "TEST123",
        Name = "Test Customer",
        Street = "Test Street 1",
        City = "Test City",
        PostalCode = "1234AB",
        CountryId = "NL"
    };

    // Act
    var response = await _cermApiClient.CreateAddressAsync(request);

    // Assert
    response.Should().NotBeNull();
    response.Success.Should().BeTrue();
    response.AddressId.Should().NotBeNullOrEmpty();
}
```

## 📚 Documentation

### Code Documentation

- **XML comments** for all public APIs
- **Clear parameter descriptions**
- **Usage examples** for complex methods
- **Exception documentation** using `<exception>` tags

### README Updates

When adding features:
- Update the feature list
- Add usage examples
- Update installation instructions if needed

## 🔄 Release Process

### Versioning

We follow [Semantic Versioning](https://semver.org/):
- **MAJOR**: Breaking changes
- **MINOR**: New features (backwards compatible)
- **PATCH**: Bug fixes (backwards compatible)

### Release Checklist

- [ ] All tests pass
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] Version number bumped
- [ ] NuGet package metadata updated

## ❓ Questions?

If you have questions about contributing:

1. **Check the documentation** first
2. **Search existing issues** and discussions
3. **Create a new issue** with the "question" label
4. **Join our discussions** on GitHub

## 🙏 Recognition

Contributors will be recognized in:
- The AUTHORS file
- Release notes
- GitHub contributors page

Thank you for helping make the CERM API Connector better!
