# CERM API Connector

[![Build Status](https://github.com/Whateverdoa/cerm-api-connector/workflows/Build/badge.svg)](https://github.com/Whateverdoa/cerm-api-connector/actions)
[![NuGet Version](https://img.shields.io/nuget/v/CermApiConnector.svg)](https://www.nuget.org/packages/CermApiConnector/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive C# .NET library for integrating with the CERM API, providing robust functionality for address management, calculations, products, and sales order operations.

## 🚀 Features

- **🔐 Authentication**: Automatic OAuth token management with caching and refresh
- **📍 Address Management**: Create, fetch, and validate addresses with bidirectional validation
- **🧮 Calculations**: Manage quotes and calculations with JSON payload support
- **📦 Product Management**: Create and manage products with complete specifications
- **📋 Sales Orders**: Comprehensive sales order creation and management
- **🌍 Multi-Environment**: Support for test and production environments
- **⚡ Performance**: Optimized with connection pooling and async operations
- **🧪 Comprehensive Testing**: Full test suite with 28+ integration tests
- **📚 Rich Documentation**: Complete API documentation and usage examples

## 📦 Installation

### NuGet Package Manager
```bash
Install-Package CermApiConnector
```

### .NET CLI
```bash
dotnet add package CermApiConnector
```

### PackageReference
```xml
<PackageReference Include="CermApiConnector" Version="1.0.0" />
```

## 🏗️ Quick Start

### 1. Configuration

Add the CERM API settings to your `appsettings.json`:

```json
{
  "CermApiSettings": {
    "Environment": "Test",
    "Test": {
      "BaseUrl": "https://vilatest-api.cerm.be/",
      "HostHeader": "vilatest-api.cerm.be"
    },
    "Production": {
      "BaseUrl": "https://vila-api.cerm.be/",
      "HostHeader": "vila-api.cerm.be"
    }
  }
}
```

Store your credentials securely using User Secrets:

```bash
dotnet user-secrets set "CermApiSettings:ClientId" "YOUR_CLIENT_ID"
dotnet user-secrets set "CermApiSettings:ClientSecret" "YOUR_CLIENT_SECRET"
dotnet user-secrets set "CermApiSettings:Username" "YOUR_USERNAME"
dotnet user-secrets set "CermApiSettings:Password" "YOUR_PASSWORD"
```

### 2. Dependency Injection

Register the CERM API client in your `Program.cs`:

```csharp
using CermApiConnector.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add CERM API client
builder.Services.AddCermApiClient(builder.Configuration);

var app = builder.Build();
```

### 3. Basic Usage

```csharp
using CermApiConnector.Services;
using CermApiConnector.Models;

public class OrderService
{
    private readonly CermApiClient _cermApiClient;

    public OrderService(CermApiClient cermApiClient)
    {
        _cermApiClient = cermApiClient;
    }

    public async Task<string> ProcessOrderAsync()
    {
        // Create an address
        var addressRequest = new CreateAddressRequest
        {
            CustomerId = "100001",
            Name = "Customer Name",
            Street = "Main Street 123",
            PostalCode = "1234AB",
            City = "Amsterdam",
            CountryId = "NL",
            Email = "customer@example.com",
            IsDeliveryAddress = true
        };

        var addressResponse = await _cermApiClient.CreateAddressAsync(addressRequest);
        
        if (addressResponse.Success)
        {
            return addressResponse.AddressId;
        }
        
        throw new Exception($"Failed to create address: {addressResponse.Error}");
    }
}
```

## 📖 Documentation

- **[API Manual](docs/CERM_API_Manual.md)** - Complete API reference and usage guide
- **[Testing Guide](docs/CERM_API_Testing_Guide.md)** - Testing procedures and troubleshooting
- **[Testing Plan](docs/CERM_API_Testing_Plan.md)** - Comprehensive testing strategy
- **[Examples](docs/examples/)** - Code examples and integration patterns

## 🧪 Testing

The library includes comprehensive tests covering all functionality:

```bash
# Run all tests
dotnet test

# Run specific test suite
dotnet test --filter "ClassName=AddressManagementTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage
- **Address Management**: 5 tests covering creation, fetching, and validation
- **Authentication**: 5 tests for token management and caching
- **Calculations**: 5 tests for quote and calculation operations
- **Products**: 4 tests for product creation and management
- **Sales Orders**: 5 tests for order creation and validation
- **Integration**: 4 tests for end-to-end workflows

## 🔧 Configuration

### Environment Switching

Switch between test and production environments:

```csharp
// Programmatically
services.Configure<CermApiSettings>(options => 
{
    options.Environment = "Production";
});

// Via configuration
dotnet user-secrets set "CermApiSettings:Environment" "Production"
```

### Advanced Configuration

```json
{
  "CermApiSettings": {
    "Environment": "Production",
    "ConnectionTimeout": 30,
    "RetryAttempts": 3,
    "CacheTokenDuration": 3600
  }
}
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 8.0 SDK
3. Set up user secrets for testing
4. Run tests to verify setup

```bash
git clone https://github.com/Whateverdoa/cerm-api-connector.git
cd cerm-api-connector
dotnet restore
dotnet test
```

## 📋 Requirements

- .NET 8.0 or later
- Valid CERM API credentials
- Internet connectivity for API calls

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Issues**: [GitHub Issues](https://github.com/Whateverdoa/cerm-api-connector/issues)
- **Documentation**: [Wiki](https://github.com/Whateverdoa/cerm-api-connector/wiki)
- **Examples**: [Sample Projects](samples/)

## 🏷️ Versioning

We use [SemVer](http://semver.org/) for versioning. For available versions, see the [tags on this repository](https://github.com/Whateverdoa/cerm-api-connector/tags).

## 👥 Authors

- **Mike ten Hoonte** - *Initial work* - [@Whateverdoa](https://github.com/Whateverdoa)

## 🙏 Acknowledgments

- Vila Etiketten for supporting the development
- CERM for providing the API platform
- The .NET community for excellent tooling and libraries
