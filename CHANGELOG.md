# Changelog

All notable changes to the CERM API Connector will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project setup and repository structure
- Comprehensive documentation and contribution guidelines

## [1.0.0] - 2025-01-01

### Added
- **Core CERM API Client** with OAuth authentication
- **Address Management** functionality
  - Create addresses with `CreateAddressAsync`
  - Fetch address IDs with `FetchAddressIdAsync`
  - Validate addresses with `ValidateAddressIdAsync`
  - Bidirectional address validation with `ValidateAddressBidirectionalAsync`
  - JSON-based address creation with `CreateAddressWithJsonAsync`
- **Calculation Management** functionality
  - Fetch quote IDs with `FetchQuoteIdAsync`
  - Create calculations with JSON payloads using `CreateCalculationWithJsonAsync`
- **Product Management** functionality
  - Create products with parameters using `CreateProductAsync`
  - Create products with JSON payloads using `CreateProductWithJsonAsync`
- **Sales Order Management** functionality
  - Create sales orders with `CreateSalesOrderAsync`
  - Create sales orders with JSON payloads using `CreateSalesOrderWithJsonAsync`
- **Multi-Environment Support**
  - Test environment: `https://vilatest-api.cerm.be/`
  - Production environment: `https://vila-api.cerm.be/`
  - Easy environment switching via configuration
- **Authentication & Security**
  - Automatic OAuth token management
  - Token caching and refresh
  - Secure credential storage via User Secrets
- **Comprehensive Testing Suite**
  - 28+ integration tests covering all functionality
  - Address management tests (5 tests)
  - Authentication tests (5 tests)
  - Calculation tests (5 tests)
  - Product tests (4 tests)
  - Sales order tests (5 tests)
  - Integration workflow tests (4 tests)
  - Performance and error handling tests
- **Configuration & Extensions**
  - Dependency injection support with `AddCermApiClient`
  - Flexible configuration via `appsettings.json`
  - User secrets integration for secure credential management
- **Models & DTOs**
  - `TokenResponse` for authentication
  - `AddressIdResponse` and `AddressDetailsResponse` for address operations
  - `CreateAddressRequest` for address creation
  - `QuoteIdResponse` for calculation operations
  - `ProductIdResponse` for product operations
  - `SalesOrderIdResponse` for sales order operations
  - `AddressValidationResult` for comprehensive address validation
- **Documentation**
  - Complete API manual with usage examples
  - Comprehensive testing guide and procedures
  - Detailed testing plan and strategy
  - Integration examples and best practices
- **Development Tools**
  - User secrets helper for environment management
  - Comprehensive logging throughout the library
  - Error handling with detailed error messages
  - Performance optimizations with async/await patterns

### Technical Details
- **Target Framework**: .NET 9.0
- **Dependencies**:
  - Microsoft.Extensions.Configuration (8.0.0)
  - Microsoft.Extensions.DependencyInjection.Abstractions (8.0.2)
  - Microsoft.Extensions.Http (8.0.1)
  - Microsoft.Extensions.Logging.Abstractions (8.0.2)
  - Microsoft.Extensions.Options (8.0.2)
  - System.Text.Json (8.0.5)
- **Test Framework**: xUnit with FluentAssertions
- **Package Features**:
  - NuGet package ready with proper metadata
  - XML documentation generation
  - MIT License
  - Comprehensive README and documentation

### Security
- No hardcoded credentials in source code
- Secure credential management via User Secrets
- Environment-specific configuration support
- Proper error handling without exposing sensitive information

### Performance
- Async/await patterns throughout
- HTTP client connection pooling
- Token caching to reduce authentication overhead
- Optimized JSON serialization/deserialization

---

## Release Notes

### Version 1.0.0 - Initial Release

This is the initial release of the CERM API Connector, providing a comprehensive C# .NET library for integrating with the CERM API. The library has been thoroughly tested and includes complete documentation and examples.

**Key Highlights:**
- Full CERM API integration with address, calculation, product, and sales order management
- Comprehensive test suite with 28+ tests ensuring reliability
- Multi-environment support for both test and production
- Secure authentication with automatic token management
- Easy integration with dependency injection
- Complete documentation and examples

**Getting Started:**
1. Install via NuGet: `dotnet add package CermApiConnector`
2. Configure your credentials via User Secrets
3. Register the service: `services.AddCermApiClient(configuration)`
4. Start using the `CermApiClient` in your applications

For detailed usage instructions, see the [README.md](README.md) and [API Manual](docs/CERM_API_Manual.md).

---

## Support

For questions, issues, or contributions, please visit our [GitHub repository](https://github.com/Whateverdoa/cerm-api-connector).
