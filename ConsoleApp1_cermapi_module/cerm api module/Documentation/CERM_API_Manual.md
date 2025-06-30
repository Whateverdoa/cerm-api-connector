# CERM API Connector - User Manual

## Overview

The CERM API Connector is a C# library that provides a clean, extensible interface for interacting with the CERM API. It supports both test and production environments and encapsulates authentication, endpoint management, and ID retrieval (address, quote, product, sales order, etc.).

## Table of Contents

1. [Configuration](#configuration)
2. [Environment Management](#environment-management)
3. [Authentication](#authentication)
4. [API Operations](#api-operations)
5. [Error Handling](#error-handling)
6. [Examples](#examples)
7. [Troubleshooting](#troubleshooting)

## Configuration

### Settings Structure

The CERM API Connector uses the following configuration structure in `appsettings.json`:

```json
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
  "ClientId": "[Use User Secrets for this]",
  "ClientSecret": "[Use User Secrets for this]",
  "Username": "[Use User Secrets for this]",
  "Password": "[Use User Secrets for this]",
  "Paths": {
    "Token": "oauth/token",
    "FetchAddressId": "custom-api/export/fetchaddressid",
    "CreateAddress": "address-api/v1/addresses",
    "FetchAddressById": "address-api/v1/addresses/{id}",
    "FetchCalculationId": "custom-api/export/fetchcalculationid",
    "CreateQuote": "quote-api/v1/calculations",
    "CreateProduct": "product-api/v1/calculations/{calculationId}/products",
    "CreateSalesOrder": "sales-order-api/v1/customers/{customerId}/contacts/{contactId}/sales-orders/order"
  }
}
```

### Credentials Management

For security reasons, sensitive credentials should be stored in User Secrets, not in `appsettings.json`. The following credentials are required:

- `ClientId`: The OAuth client ID for the CERM API
- `ClientSecret`: The OAuth client secret for the CERM API
- `Username`: The username for the CERM API
- `Password`: The password for the CERM API

To load credentials from a `.env` file into User Secrets, use the `LoadCermCredentials` run configuration in Rider, or run:

```bash
dotnet run load-cerm-credentials
```

The `.env` file should contain:

```
CERM_CLIENT_ID=your_client_id
CERM_CLIENT_SECRET=your_client_secret
CERM_USERNAME=your_username
CERM_PASSWORD=your_password
```

## Environment Management

The CERM API Connector supports two environments:

1. **Test Environment**:
   - Base URL: https://vilatest-api.cerm.be/
   - Host Header: vilatest-api.cerm.be

2. **Production Environment**:
   - Base URL: https://vila-api.cerm.be/
   - Host Header: vila-api.cerm.be

### Switching Environments

To switch between environments, use one of the following methods:

#### Using Rider Run Configurations

- **SetCermEnvironmentTest**: Sets the environment to "Test"
- **SetCermEnvironmentProduction**: Sets the environment to "Production"

#### Using Command Line

```bash
# Switch to Test environment
dotnet run set-cerm-environment Test

# Switch to Production environment
dotnet run set-cerm-environment Production
```

#### Programmatically

```csharp
// Inject UserSecretsHelper
private readonly UserSecretsHelper _userSecretsHelper;

// Switch to Test environment
_userSecretsHelper.SetCermApiEnvironment("Test");

// Switch to Production environment
_userSecretsHelper.SetCermApiEnvironment("Production");
```

## Authentication

The CERM API Connector handles authentication automatically. It obtains an OAuth token when needed and caches it for subsequent requests. The token is refreshed automatically when it expires.

## API Operations

### Address Management

#### Fetching an Address ID

```csharp
// Inject CermApiClient
private readonly CermApiClient _cermApiClient;

// Fetch an address ID
var addressIdResponse = await _cermApiClient.FetchAddressIdAsync(
    customerId: "CUSTOMER123",
    postalCode: "1234 AB",
    street: "123 Main Street",
    city: "Amsterdam",
    countryId: "NL"
);

if (addressIdResponse.Success)
{
    string addressId = addressIdResponse.AddressId;
    // Use the address ID
}
else
{
    // Handle error
    string errorMessage = addressIdResponse.Error;
}
```

#### Creating an Address

```csharp
// Create an address request
var createAddressRequest = new CreateAddressRequest
{
    CustomerId = "CUSTOMER123",
    Name = "Test Customer",
    Street = "123 Main Street",
    Number = "1",
    PostalCode = "1234 AB",
    City = "Amsterdam",
    CountryId = "NL",
    Email = "test@example.com",
    Phone = "1234567890",
    IsDeliveryAddress = true,
    IsInvoiceAddress = false
};

// Create the address
var createAddressResponse = await _cermApiClient.CreateAddressAsync(createAddressRequest);

if (createAddressResponse.Success)
{
    string addressId = createAddressResponse.AddressId;
    // Use the address ID
}
else
{
    // Handle error
    string errorMessage = createAddressResponse.Error;
}
```

#### Validating an Address ID

```csharp
// Check if an address ID exists (lightweight check)
string addressId = "443675";
bool exists = await _cermApiClient.AddressIdExistsAsync(addressId);

if (exists)
{
    // Address ID is valid
}
```

#### Getting Address Details by ID

```csharp
// Get full address details by address ID
string addressId = "443675";
var addressDetails = await _cermApiClient.ValidateAddressIdAsync(addressId);

if (addressDetails.Success && addressDetails.Exists)
{
    string customerId = addressDetails.CustomerId;
    string street = addressDetails.Street;
    string city = addressDetails.City;
    string postalCode = addressDetails.PostalCode;
    string country = addressDetails.Country;
    bool isActive = addressDetails.IsActive;
    // Use the address details
}
else
{
    // Address ID doesn't exist or error occurred
    string error = addressDetails.Error;
}
```

#### Bidirectional Address Validation

```csharp
// Comprehensive validation that checks both directions:
// 1. Address details → Address ID
// 2. Address ID → Address details
// 3. Compares original vs validated details
var validationResult = await _cermApiClient.ValidateAddressBidirectionalAsync(
    customerId: "100001",
    postalCode: "4814TT",
    street: "Main Street",
    city: "Breda",
    countryId: "NL"
);

if (validationResult.Success)
{
    bool addressIdFound = validationResult.AddressIdFound;
    bool addressIdValid = validationResult.AddressIdValid;
    bool detailsMatch = validationResult.AddressDetailsMatch;
    string addressId = validationResult.AddressId;

    if (addressIdFound && addressIdValid && detailsMatch)
    {
        // Perfect match - address is fully validated
        // Use validationResult.AddressId
    }
    else if (addressIdFound && addressIdValid)
    {
        // Address exists but some details differ
        // Check validationResult.ValidatedAddressDetails for differences
    }
}
else
{
    // Validation failed
    string error = validationResult.Error;
}
```

### Quote Management

#### Fetching a Quote ID

```csharp
// Fetch a quote ID
var quoteIdResponse = await _cermApiClient.FetchQuoteIdAsync(
    customerId: "CUSTOMER123",
    productCode: "PRODUCT456"
);

if (quoteIdResponse.Success)
{
    string calculationId = quoteIdResponse.CalculationId;
    // Use the calculation ID
}
else
{
    // Handle error
    string errorMessage = quoteIdResponse.Error;
}
```

### Product Management

#### Creating a Product

```csharp
// Create a product
var productIdResponse = await _cermApiClient.CreateProductAsync(
    calculationId: "CALC789",
    productName: "Test Product",
    quantity: 100,
    unitPrice: 25.50m
);

if (productIdResponse.Success)
{
    string productId = productIdResponse.ProductId;
    // Use the product ID
}
else
{
    // Handle error
    string errorMessage = productIdResponse.Error;
}
```

### Sales Order Management

#### Creating a Sales Order

```csharp
// Create a sales order
var orderData = new
{
    orderNumber = "ORDER123",
    orderDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
    products = new[]
    {
        new
        {
            productId = "PRODUCT456",
            quantity = 100,
            unitPrice = 25.50m
        }
    }
};

var salesOrderIdResponse = await _cermApiClient.CreateSalesOrderAsync(
    customerId: "CUSTOMER123",
    contactId: "CONTACT789",
    orderData: orderData
);

if (salesOrderIdResponse.Success)
{
    string salesOrderId = salesOrderIdResponse.SalesOrderId;
    // Use the sales order ID
}
else
{
    // Handle error
    string errorMessage = salesOrderIdResponse.Error;
}
```

## Error Handling

The CERM API Connector provides detailed error information in the response objects. Each response object has:

- `Success`: A boolean indicating whether the operation was successful
- `Error`: A string containing the error message, if any
- `Message`: A string containing additional information, if any

Example error handling:

```csharp
var response = await _cermApiClient.FetchAddressIdAsync(...);

if (!response.Success)
{
    _logger.LogError("Failed to fetch address ID: {Error}", response.Error);
    // Handle the error appropriately
}
```

## Examples

### Complete Address Processing Example

```csharp
public async Task ProcessAddressAsync(string customerId, string postalCode, string street, string city, string countryId)
{
    try
    {
        // First, try to fetch the address ID
        var fetchResponse = await _cermApiClient.FetchAddressIdAsync(
            customerId, postalCode, street, city, countryId);

        if (fetchResponse.Success)
        {
            _logger.LogInformation("Address ID found: {AddressId}", fetchResponse.AddressId);
            return fetchResponse.AddressId;
        }

        _logger.LogInformation("Address not found, creating a new one...");

        // If not found, create a new address
        var createRequest = new CreateAddressRequest
        {
            CustomerId = customerId,
            Name = "Customer Name", // Replace with actual name
            Street = street,
            Number = "1", // Extract from street if possible
            PostalCode = postalCode,
            City = city,
            CountryId = countryId,
            Email = "customer@example.com", // Replace with actual email
            Phone = "1234567890", // Replace with actual phone
            IsDeliveryAddress = true,
            IsInvoiceAddress = false
        };

        var createResponse = await _cermApiClient.CreateAddressAsync(createRequest);

        if (createResponse.Success)
        {
            _logger.LogInformation("New address created with ID: {AddressId}", createResponse.AddressId);
            return createResponse.AddressId;
        }
        else
        {
            _logger.LogError("Failed to create address: {Error}", createResponse.Error);
            return null;
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing address: {Message}", ex.Message);
        return null;
    }
}
```

## Troubleshooting

### Common Issues

1. **Authentication Failures**:
   - Ensure your credentials are correct in User Secrets
   - Check that you're using the correct environment (Test or Production)
   - Verify that your account has the necessary permissions

2. **Connection Issues**:
   - Check your network connectivity
   - Verify that the CERM API is accessible from your network
   - Check if there are any firewall rules blocking the connection

3. **API Response Errors**:
   - Check the error message in the response
   - Verify that you're sending the correct data
   - Consult the CERM API documentation for specific error codes

### Logging

The CERM API Connector logs detailed information about its operations. To enable verbose logging, add the following to your `appsettings.json`:

```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "aws_b2b_mod1.Services.CermApiClient": "Debug"
  }
}
```

This will provide more detailed logs about the CERM API operations, including request and response details.

### Getting Help

If you encounter issues that you cannot resolve, please contact the development team with the following information:

1. A description of the issue
2. The environment you're using (Test or Production)
3. The relevant logs
4. Any error messages you're receiving
5. Steps to reproduce the issue

---

## Appendix: API Reference

### CermApiClient Methods

| Method | Description | Parameters | Return Type |
|--------|-------------|------------|-------------|
| `GetTokenAsync()` | Gets an OAuth token | None | `Task<TokenResponse>` |
| `FetchAddressIdAsync()` | Fetches an address ID | `customerId`, `postalCode`, `street`, `city`, `countryId` | `Task<AddressIdResponse>` |
| `CreateAddressAsync()` | Creates a new address | `CreateAddressRequest` | `Task<AddressIdResponse>` |
| `ValidateAddressIdAsync()` | Validates address ID and gets details | `addressId` | `Task<AddressDetailsResponse>` |
| `AddressIdExistsAsync()` | Checks if address ID exists | `addressId` | `Task<bool>` |
| `ValidateAddressBidirectionalAsync()` | Comprehensive address validation | `customerId`, `postalCode`, `street`, `city`, `countryId` | `Task<AddressValidationResult>` |
| `FetchQuoteIdAsync()` | Fetches a quote ID | `customerId`, `productCode` | `Task<QuoteIdResponse>` |
| `CreateProductAsync()` | Creates a new product | `calculationId`, `productName`, `quantity`, `unitPrice` | `Task<ProductIdResponse>` |
| `CreateSalesOrderAsync()` | Creates a new sales order | `customerId`, `contactId`, `orderData` | `Task<SalesOrderIdResponse>` |

### Response Objects

| Object | Properties |
|--------|------------|
| `TokenResponse` | `AccessToken`, `TokenType`, `ExpiresIn`, `Scope`, `Jti`, `ExpiresAt`, `IsExpired` |
| `AddressIdResponse` | `Id`, `AddressId`, `Success`, `Message`, `Error` |
| `QuoteIdResponse` | `Id`, `CalculationId`, `Success`, `Message`, `Error` |
| `ProductIdResponse` | `Id`, `ProductId`, `Success`, `Message`, `Error` |
| `SalesOrderIdResponse` | `Id`, `SalesOrderId`, `Success`, `Message`, `Error` |

### Request Objects

| Object | Properties |
|--------|------------|
| `CreateAddressRequest` | `CustomerId`, `Name`, `Street`, `Number`, `PostalCode`, `City`, `CountryId`, `Email`, `Phone`, `IsDeliveryAddress`, `IsInvoiceAddress` |
