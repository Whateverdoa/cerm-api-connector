# CERM API Module

This module contains all the components needed to integrate with the CERM API. It provides a complete, self-contained solution for authentication, address management, quote creation, product creation, and sales order management.

## Module Structure

```
cerm api module/
├── Configuration/
│   └── CermApiSettings.cs          # API configuration and environment settings
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # Dependency injection extensions
├── Models/
│   ├── TokenResponse.cs            # OAuth token response model
│   ├── AddressIdResponse.cs        # Address ID response model
│   ├── AddressDetailsResponse.cs   # Address details response model
│   ├── AddressValidationResult.cs  # Address validation result model
│   ├── ProductIdResponse.cs        # Product ID response model
│   ├── QuoteIdResponse.cs          # Quote/Calculation ID response model
│   ├── SalesOrderIdResponse.cs     # Sales order ID response model
│   ├── CalculationIdResponse.cs    # Calculation ID response model
│   └── CermIdentifier.cs           # CERM identifier tracking model
├── Services/
│   └── CermApiClient.cs            # Main API client service
├── Tests/
│   ├── CermApiTest.cs              # Basic API functionality tests
│   ├── CermApiAddressTest.cs       # Address-specific tests
│   └── CermApiAddressValidationTest.cs  # Address validation tests
├── Documentation/
│   ├── CERM_API_Manual.md          # Complete user manual
│   └── CERM_API_Testing_Guide.md   # Testing procedures and examples
└── README.md                       # This file
```

## Key Features

### 🔐 Authentication
- Automatic OAuth token management
- Token caching and refresh
- Support for test and production environments

### 🏠 Address Management
- Fetch existing address IDs
- Create new addresses
- Bidirectional address validation
- Address details retrieval

### 📋 Quote & Product Management
- Create calculations/quotes
- Create products with JSON payloads
- Link products to calculations

### 📦 Sales Order Management
- Create sales orders with JSON payloads
- Customer and contact management

### 🌍 Environment Support
- Test environment: `vilatest-api.cerm.be`
- Production environment: `vila-api.cerm.be`
- Easy environment switching via configuration

## Quick Start

### 1. Configuration

Add to your `appsettings.json`:

```json
{
  "CermApiSettings": {
    "Environment": "Test",
    "ClientId": "[Use User Secrets]",
    "ClientSecret": "[Use User Secrets]",
    "Username": "[Use User Secrets]",
    "Password": "[Use User Secrets]"
  }
}
```

### 2. Dependency Injection

In your `Program.cs` or `Startup.cs`:

```csharp
using aws_b2b_mod1.Extensions;

// Add CERM API client
services.AddCermApiClient(configuration);
```

### 3. Usage Example

```csharp
public class MyService
{
    private readonly CermApiClient _cermApiClient;

    public MyService(CermApiClient cermApiClient)
    {
        _cermApiClient = cermApiClient;
    }

    public async Task<string> GetAddressIdAsync()
    {
        var response = await _cermApiClient.FetchAddressIdAsync(
            customerId: "104793",
            postalCode: "4814TT",
            street: "Main Street 1",
            city: "Breda",
            countryId: "NL"
        );

        return response.Success ? response.AddressId : string.Empty;
    }
}
```

## Testing

The module includes comprehensive tests:

- **CermApiTest.cs**: Basic token and API functionality
- **CermApiAddressTest.cs**: Address creation and retrieval
- **CermApiAddressValidationTest.cs**: Bidirectional address validation

Run tests using your preferred test runner or IDE.

## Documentation

- **CERM_API_Manual.md**: Complete user manual with examples
- **CERM_API_Testing_Guide.md**: Testing procedures and troubleshooting

## Dependencies

- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging
- Microsoft.Extensions.Http
- System.Text.Json

## Environment Variables / User Secrets

Store sensitive configuration in user secrets:

```json
{
  "CermApiSettings:ClientId": "your-client-id",
  "CermApiSettings:ClientSecret": "your-client-secret",
  "CermApiSettings:Username": "your-username",
  "CermApiSettings:Password": "your-password"
}
```

## Support

For issues or questions, refer to the documentation in the `Documentation/` folder or check the test files for usage examples.
