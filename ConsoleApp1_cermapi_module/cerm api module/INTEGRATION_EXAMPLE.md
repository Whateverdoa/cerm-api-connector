# CERM API Module Integration Example

This document shows how to integrate the CERM API module into your .NET project.

## Step 1: Copy Files to Your Project

Copy the module files to your project structure:

```
YourProject/
├── Configuration/
│   └── CermApiSettings.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
├── Models/
│   ├── TokenResponse.cs
│   ├── AddressIdResponse.cs
│   ├── AddressDetailsResponse.cs
│   ├── AddressValidationResult.cs
│   ├── ProductIdResponse.cs
│   ├── QuoteIdResponse.cs
│   ├── SalesOrderIdResponse.cs
│   ├── CalculationIdResponse.cs
│   └── CermIdentifier.cs
└── Services/
    └── CermApiClient.cs
```

## Step 2: Update Namespaces

Update the namespace in all copied files to match your project:

```csharp
// Change from:
namespace aws_b2b_mod1.Models;

// To:
namespace YourProject.Models;
```

## Step 3: Add NuGet Packages

Add required packages to your project:

```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="System.Text.Json" Version="8.0.0" />
```

## Step 4: Configure Services

In your `Program.cs`:

```csharp
using YourProject.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add CERM API client
builder.Services.AddCermApiClient(builder.Configuration);

var app = builder.Build();
```

## Step 5: Add Configuration

In your `appsettings.json`:

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
    },
    "ClientId": "[Use User Secrets]",
    "ClientSecret": "[Use User Secrets]",
    "Username": "[Use User Secrets]",
    "Password": "[Use User Secrets]",
    "Paths": {
      "Token": "oauth/token",
      "FetchAddressId": "custom-api/export/fetchaddressid",
      "CreateAddress": "address-api/v1/addresses",
      "FetchAddressById": "address-api/v1/addresses/{id}",
      "FetchCalculationId": "custom-api/export/fetchcalculationid",
      "CreateCalculation": "quote-api/v1/calculations",
      "CreateQuote": "quote-api/v1/calculations",
      "CreateProduct": "product-api/v1/calculations/{calculationId}/products",
      "CreateSalesOrder": "sales-order-api/v1/customers/{customerId}/contacts/{contactId}/sales-orders/order"
    }
  }
}
```

## Step 6: Set User Secrets

```bash
dotnet user-secrets set "CermApiSettings:ClientId" "your-client-id"
dotnet user-secrets set "CermApiSettings:ClientSecret" "your-client-secret"
dotnet user-secrets set "CermApiSettings:Username" "your-username"
dotnet user-secrets set "CermApiSettings:Password" "your-password"
```

## Step 7: Use in Your Services

```csharp
using YourProject.Services;
using YourProject.Models;

public class OrderService
{
    private readonly CermApiClient _cermApiClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(CermApiClient cermApiClient, ILogger<OrderService> logger)
    {
        _cermApiClient = cermApiClient;
        _logger = logger;
    }

    public async Task<string> ProcessOrderAddressAsync(string customerId, string postalCode, 
        string street, string city, string country)
    {
        try
        {
            // Try to fetch existing address
            var addressResponse = await _cermApiClient.FetchAddressIdAsync(
                customerId, postalCode, street, city, country);

            if (addressResponse.Success && !string.IsNullOrEmpty(addressResponse.AddressId))
            {
                _logger.LogInformation("Found existing address: {AddressId}", addressResponse.AddressId);
                return addressResponse.AddressId;
            }

            // Create new address if not found
            var createResponse = await _cermApiClient.CreateAddressAsync(
                customerId, postalCode, street, city, country, 
                "Customer Address", "", "", true, true, country, new List<object>());

            if (createResponse.Success)
            {
                _logger.LogInformation("Created new address: {AddressId}", createResponse.AddressId);
                return createResponse.AddressId;
            }

            _logger.LogError("Failed to create address: {Error}", createResponse.Error);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing address");
            return string.Empty;
        }
    }
}
```

## Step 8: Testing

Copy the test files and update their namespaces to test your integration:

```csharp
// In your test project
using YourProject.Services;
using YourProject.Models;

[Test]
public async Task TestCermApiConnection()
{
    var token = await _cermApiClient.GetTokenAsync();
    Assert.IsTrue(!string.IsNullOrEmpty(token.AccessToken));
}
```

## Environment Switching

To switch between test and production environments, update the configuration:

```json
{
  "CermApiSettings": {
    "Environment": "Production"  // Change from "Test" to "Production"
  }
}
```

## Troubleshooting

1. **Authentication Issues**: Verify your credentials in user secrets
2. **Network Issues**: Check firewall and proxy settings
3. **API Errors**: Enable debug logging to see detailed API responses
4. **Token Expiry**: The client automatically handles token refresh

For more detailed troubleshooting, see `Documentation/CERM_API_Testing_Guide.md`.
