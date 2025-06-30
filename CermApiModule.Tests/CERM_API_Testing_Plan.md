# CERM API Testing Plan

## Overview

This document outlines the comprehensive testing strategy for the CERM API integration using xUnit test framework. The tests are designed to validate all aspects of the CERM API functionality including address management, calculations, products, sales orders, and end-to-end integration workflows.

## Test Architecture

### Test Base Infrastructure

- **TestBase**: Abstract base class providing common setup and utilities
- **TestDataProvider**: Static class providing test data from F003ADB6G8.json
- **OrderTestData**: Model representing the test order data structure
- **Environment Configuration**: Uses .env files and appsettings.json for configuration

### Test Data Source

All tests use data from the F003ADB6G8.json file, which contains:
- Customer: Vila Etiketten (ID: 100001)
- Product: Roll Stickers (30.0mm Circle, 1000 quantity)
- Address: Rue Saint Donat 6, 5640 Mettet, BE
- Contact: Guillaume Allard (DWD@drukwerkdeal.nl)
- Delivery: 2025-03-07
- Unit Price: €37.18

## Test Suites

### 1. AddressManagementTests.cs

**Purpose**: Tests CERM API address operations

**Key Tests**:
- `FetchAddressIdAsync_WithOrderData_ReturnsAddressIdOrNull`
- `CreateAddressAsync_WithOrderData_ReturnsValidAddressId`
- `AddressValidation_WithCreatedAddress_ValidatesCorrectly`
- `CreateAndFetchAddress_EndToEndWorkflow_WorksCorrectly`
- `AddressOperations_PerformanceTest_CompletesWithinTimeout`

**Corrected Implementation**:
- Uses `CreateAddressRequest` objects for address creation
- Properly handles `AddressIdResponse` return types
- Validates response success and extracts address IDs correctly

### 2. CalculationTests.cs

**Purpose**: Tests CERM API calculation/quote management

**Key Tests**:
- `FetchQuoteIdAsync_WithOrderData_ReturnsQuoteIdOrNull`
- `CreateCalculationWithJsonAsync_WithOrderData_ReturnsValidCalculationId`
- `CalculationCreation_WithCompleteOrderData_CreatesSuccessfully`
- `CalculationOperations_PerformanceTest_CompletesWithinTimeout`

**Corrected Implementation**:
- Uses `CreateCalculationWithJsonAsync` method correctly
- Handles `QuoteIdResponse` return types properly
- Extracts `CalculationId` from response objects

### 3. ProductTests.cs

**Purpose**: Tests CERM API product management

**Key Tests**:
- `CreateProductAsync_WithParameters_ReturnsValidProductId`
- `CreateProductWithJsonAsync_WithOrderData_ReturnsValidProductId`
- `ProductCreation_WithCompleteSpecifications_CreatesSuccessfully`
- `ProductOperations_PerformanceTest_CompletesWithinTimeout`

**Corrected Implementation**:
- Uses correct `CreateProductAsync` signature (4 parameters: calculationId, productName, quantity, unitPrice)
- Properly handles `ProductIdResponse` return types
- Links products to calculations using correct calculation IDs

### 4. SalesOrderTests.cs

**Purpose**: Tests CERM API sales order management

**Key Tests**:
- `CreateSalesOrderWithJsonAsync_WithOrderData_ReturnsValidSalesOrderId`
- `SalesOrderCreation_WithCompleteOrderData_CreatesSuccessfully`
- `SalesOrderCreation_WithDifferentCustomerAndContact_WorksCorrectly`
- `SalesOrderOperations_PerformanceTest_CompletesWithinTimeout`

**Corrected Implementation**:
- Uses `CreateSalesOrderWithJsonAsync` with correct 3-parameter signature
- Handles `SalesOrderIdResponse` return types properly
- Validates response success and extracts sales order IDs

### 5. IntegrationTests.cs

**Purpose**: Tests complete end-to-end workflows

**Key Tests**:
- `CompleteOrderWorkflow_EndToEnd_CreatesAllEntitiesSuccessfully`
- `WorkflowErrorHandling_WithInvalidData_HandlesGracefully`
- `ConcurrentWorkflows_MultipleOrders_HandleCorrectly`
- `DataConsistency_AcrossEndpoints_MaintainsIntegrity`

**Corrected Implementation**:
- Uses `CreateAddressWithJsonAsync` for JSON-based address creation
- Properly chains entity creation (Address → Calculation → Product → Sales Order)
- Handles all response types correctly and extracts IDs for subsequent operations

## Key Corrections Made

### 1. Method Signature Fixes

**Before**: Incorrect parameter counts and types
```csharp
// Wrong - 6 parameters
CermApiClient.CreateProductAsync(calculationId, name, quantity, price, width, height)

// Wrong - JSON string to method expecting object
CermApiClient.CreateAddressAsync(jsonString)
```

**After**: Correct method signatures
```csharp
// Correct - 4 parameters
CermApiClient.CreateProductAsync(calculationId, name, quantity, price)

// Correct - JSON method for JSON strings
CermApiClient.CreateAddressWithJsonAsync(jsonString)

// Correct - Object method for request objects
CermApiClient.CreateAddressAsync(addressRequest)
```

### 2. Response Type Handling

**Before**: Expecting string returns
```csharp
var addressId = await CermApiClient.CreateAddressAsync(request);
addressId.Should().NotBeEmpty(); // Wrong - addressId is AddressIdResponse
```

**After**: Proper response object handling
```csharp
var addressResponse = await CermApiClient.CreateAddressAsync(request);
addressResponse.Should().NotBeNull();
addressResponse.Success.Should().BeTrue();
addressResponse.AddressId.Should().NotBeNullOrEmpty();
```

### 3. FluentAssertions Usage

**Before**: Incorrect assertion methods
```csharp
salesOrderId.Should().NotBeEmpty(); // Wrong for object types
```

**After**: Correct assertion methods
```csharp
salesOrderResponse.SalesOrderId.Should().NotBeNullOrEmpty();
```

## Test Execution

### Prerequisites

1. **Environment Configuration**: Ensure .env file contains valid CERM API credentials
2. **Test Data**: F003ADB6G8.json data is embedded in TestDataProvider
3. **Network Access**: Tests require internet connectivity to CERM API endpoints

### Running Tests

```bash
# Build the test project
dotnet build CermApiModule.Tests/CermApiModule.Tests.csproj

# Run all tests
dotnet test CermApiModule.Tests/CermApiModule.Tests.csproj

# Run specific test suite
dotnet test CermApiModule.Tests/CermApiModule.Tests.csproj --filter "ClassName=AddressManagementTests"

# Run with verbose output
dotnet test CermApiModule.Tests/CermApiModule.Tests.csproj --logger "console;verbosity=detailed"
```

### Test Configuration

Tests use the following configuration hierarchy:
1. `.env` file (for local development)
2. `appsettings.json` (for default settings)
3. User secrets (for sensitive data)
4. Environment variables (for CI/CD)

## Performance Expectations

- **Address Operations**: < 30 seconds per operation
- **Calculation Creation**: < 30 seconds per operation
- **Product Creation**: < 30 seconds per operation
- **Sales Order Creation**: < 30 seconds per operation
- **End-to-End Workflow**: < 5 minutes total
- **Concurrent Operations**: < 2 minutes for 3 parallel operations

## Error Handling

Tests include comprehensive error handling for:
- Network timeouts
- API authentication failures
- Invalid data scenarios
- Concurrent operation conflicts
- Resource not found scenarios

## Maintenance

### Adding New Tests

1. Inherit from `TestBase` class
2. Use `TestDataProvider.GetDefaultOrderData()` for consistent test data
3. Follow naming convention: `MethodName_Scenario_ExpectedResult`
4. Include proper logging and assertions
5. Handle response objects correctly

### Updating Test Data

1. Modify `TestDataProvider` class for new test scenarios
2. Ensure backward compatibility with existing tests
3. Update documentation to reflect changes

## Conclusion

The corrected test implementation provides comprehensive coverage of CERM API functionality while following proper C# testing patterns and correctly using the actual CermApiClient method signatures. All compilation errors have been resolved, and the tests are ready for execution.
