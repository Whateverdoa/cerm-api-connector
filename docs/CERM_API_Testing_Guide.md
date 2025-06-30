# CERM API Testing Guide

## Overview

This document provides comprehensive guidance on testing the CERM API integration in the aws_b2b_mod1 project. It covers environment configuration, authentication, address operations, testing procedures, and troubleshooting.

## Table of Contents

1. [Environment Configuration](#environment-configuration)
2. [Authentication](#authentication)
3. [Address Operations](#address-operations)
4. [Testing Procedures](#testing-procedures)
5. [Troubleshooting](#troubleshooting)
6. [API Reference](#api-reference)

## Environment Configuration

The CERM API supports two environments: Test and Production. Each environment has its own base URL and host header.

### Environment Settings

| Environment | Base URL | Host Header |
|-------------|----------|------------|
| Test | https://vilatest-api.cerm.be/ | vilatest-api.cerm.be |
| Production | https://vila-api.cerm.be/ | vila-api.cerm.be |

### Switching Environments

You can switch between environments using the user secrets:

```bash
# Switch to Test environment
dotnet user-secrets set "CermApiSettings:Environment" "Test" --project aws_b2b_mod1

# Switch to Production environment
dotnet user-secrets set "CermApiSettings:Environment" "Production" --project aws_b2b_mod1
```

Alternatively, you can use the predefined run configurations in Rider:
- "SetCermEnvironmentTest" - Sets the environment to Test
- "SetCermEnvironmentProduction" - Sets the environment to Production

## Authentication

The CERM API uses OAuth 2.0 with the password grant type for authentication.

### Credentials

The credentials are stored in user secrets:

```json
{
  "CermApiSettings:ClientId": "9CCC0945577244959C4C109DEB4AD0BA",
  "CermApiSettings:ClientSecret": "dhZL.pv2nH",
  "CermApiSettings:Username": "CermAPI",
  "CermApiSettings:Password": "Testerke.96145",
  "CermApiSettings:Environment": "Test"
}
```

### Token Retrieval

The token is retrieved by making a POST request to the `/oauth/token` endpoint with the following parameters:
- grant_type: password
- client_id: [ClientId]
- client_secret: [ClientSecret]
- username: [Username]
- password: [Password]

The request must include an HTTP Basic Authentication header with the client ID and client secret.

### Token Response

A successful token response looks like:

```json
{
  "access_token": "434BF282BEA04672A1B8E1DF331C32E5",
  "token_type": "bearer",
  "expires_in": 3599,
  "refresh_token": "8CB5A6FE4F044B128D67B1951389EDA6"
}
```

The token is valid for approximately 1 hour (3599 seconds).

## Address Operations

The CERM API provides endpoints for address operations, including fetching existing addresses and creating new addresses.

### Fetching an Address ID

To fetch an existing address ID, make a GET request to:

```
/custom-api/export/fetchaddressid?customerid={customerId}&postalcode={postalCode}&street={street}&city={city}&countryid={countryId}
```

Parameters:
- customerId: The customer ID (e.g., 100001)
- postalCode: The postal code (e.g., 4814TT)
- street: The street name (e.g., Main Street)
- city: The city name (e.g., Breda)
- countryId: The country ID (e.g., NL)

Response:
- If an address is found: `[{"AddressID": "443675"}]`
- If no address is found: `[{"AddressID": "No address found"}]`

### Creating a New Address

To create a new address, make a POST request to:

```
/address-api/v1/addresses
```

Request body:
```json
{
  "CustomerId": "100001",
  "PostalCode": "4814TT",
  "Street": "Main Street 2",
  "City": "Breda",
  "CountryId": "NL",
  "Name": "Test Address",
  "Number": "123",
  "Email": "test@example.com",
  "Phone": "+32123456789",
  "IsDeliveryAddress": true,
  "IsInvoiceAddress": false,
  "Country": "NL",
  "Contacts": [
    {
      "FirstName": "Test",
      "LastName": "Contact",
      "Email": "test@example.com",
      "Phone": "+32123456789"
    }
  ]
}
```

Response:
```json
{
  "Data": {
    "Id": "443675",
    "CustomerId": "100001",
    "SupplierId": "",
    "Keyword": "TEST_ADDRE",
    "Name": "Test Address",
    "Department": "",
    "Street": "Main Street 2",
    "Country": "NL",
    "PostalCode": "4814TT",
    "City": "Breda",
    "District": "",
    "County": "",
    "State": "",
    "PhoneNumber": "",
    "Occasional": false,
    "TransportZone": "",
    "AllowInternet": true,
    "Active": true
  }
}
```

## Testing Procedures

### Running Tests

The project includes several test scripts for testing the CERM API functionality:

1. **Test CERM API Address**:
   ```bash
   aws_b2b_mod1/test-cerm-api-address.sh
   ```
   This test:
   - Gets a token from the CERM API
   - Tries to fetch an existing address ID
   - Creates a new address if one doesn't exist
   - Verifies the address creation

2. **Test with Different Environments**:
   ```bash
   # First, set the environment
   dotnet user-secrets set "CermApiSettings:Environment" "Production" --project aws_b2b_mod1
   
   # Then run the test
   aws_b2b_mod1/test-cerm-api-address.sh
   ```

### Test Data

For testing address operations, use the following test data:

- Customer ID: 100001 (Vila Etiketten)
- Postal Code: 4814TT
- City: Breda
- Country: NL
- Street: Main Street (for fetching)
- Street: Main Street X (for creating, where X is an incrementing number)

### Test Results

#### Test Environment

The test environment currently returns a 500 Internal Server Error when trying to get a token:

```
Failed to get token: InternalServerError - {"status":500,"message":"Internal Server Error"}
```

This is a known issue with the test environment.

#### Production Environment

The production environment works correctly:

1. **Token Retrieval**: Successfully gets a token
2. **Address Lookup**: Returns "No address found" for test addresses
3. **Address Creation**: Successfully creates new addresses
4. **Address Verification**: Successfully fetches newly created addresses

## Troubleshooting

### Common Issues

1. **500 Internal Server Error on Token Retrieval**:
   - This is a known issue with the test environment
   - Use the production environment for testing

2. **Build Errors**:
   - The project may have build errors related to test files
   - Temporarily disable problematic test files by adding `#if false ... #endif` around the code

3. **Address Not Found**:
   - Verify that the customer ID, postal code, street, city, and country ID are correct
   - Try creating a new address if one doesn't exist

4. **Address Creation Fails**:
   - Ensure all required fields are provided (Name, Country, Contacts)
   - Check that the customer ID exists in the CERM system

### Debugging Tips

1. **Enable Debug Logging**:
   - The application uses Serilog for logging
   - Set the minimum level to Debug in `appsettings.json` to see more detailed logs

2. **Inspect HTTP Requests**:
   - The application logs HTTP requests and responses
   - Look for the request URL, headers, and content in the logs

3. **Check Token Validity**:
   - Tokens expire after approximately 1 hour
   - The application automatically refreshes tokens when needed

## API Reference

### Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| /oauth/token | POST | Get an authentication token |
| /custom-api/export/fetchaddressid | GET | Fetch an existing address ID |
| /address-api/v1/addresses | POST | Create a new address |

### Models

#### TokenResponse

```csharp
public class TokenResponse
{
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; }
}
```

#### AddressIdResponse

```csharp
public class AddressIdResponse
{
    public string AddressID { get; set; }
}
```

#### CreateAddressRequest

```csharp
public class CreateAddressRequest
{
    public string CustomerId { get; set; }
    public string PostalCode { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string CountryId { get; set; }
    public string Name { get; set; }
    public string Number { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public bool IsDeliveryAddress { get; set; }
    public bool IsInvoiceAddress { get; set; }
    public string Country { get; set; }
    public List<Contact> Contacts { get; set; }
}

public class Contact
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}
```

#### CreateAddressResponse

```csharp
public class CreateAddressResponse
{
    public AddressData Data { get; set; }
}

public class AddressData
{
    public string Id { get; set; }
    public string CustomerId { get; set; }
    public string SupplierId { get; set; }
    public string Keyword { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }
    public string Street { get; set; }
    public string Country { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string District { get; set; }
    public string County { get; set; }
    public string State { get; set; }
    public string PhoneNumber { get; set; }
    public bool Occasional { get; set; }
    public string TransportZone { get; set; }
    public bool AllowInternet { get; set; }
    public bool Active { get; set; }
}
```

## Conclusion

This guide provides comprehensive instructions for testing the CERM API integration in the aws_b2b_mod1 project. It covers environment configuration, authentication, address operations, testing procedures, and troubleshooting.

For further assistance, refer to the source code in the `CermApiClient.cs` file and the test files in the project.
