using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace CermApiModule.Tests;

/// <summary>
/// Tests for CERM API sales order management functionality
/// </summary>
public class SalesOrderTests : TestBase
{
    private readonly OrderTestData _testOrderData;

    public SalesOrderTests() : base()
    {
        _testOrderData = TestDataProvider.GetDefaultOrderData();
        Logger.LogInformation("SalesOrderTests initialized with test data: {OrderData}", 
            SerializeObject(_testOrderData));
    }

    [Fact]
    public async Task CreateSalesOrderWithJsonAsync_WithOrderData_ReturnsValidSalesOrderId()
    {
        // Arrange
        LogTestStart(nameof(CreateSalesOrderWithJsonAsync_WithOrderData_ReturnsValidSalesOrderId));
        var uniqueTestId = GenerateTestId();
        var customerId = TestDataProvider.GetTestCustomerId();
        var contactId = "1"; // Default contact ID for testing

        try
        {
            // Create sales order JSON payload
            var salesOrderJson = TestDataProvider.CreateSalesOrderJsonPayload(_testOrderData, customerId, contactId);
            
            // Add unique identifier to avoid conflicts
            var salesOrderData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(salesOrderJson);
            var modifiedSalesOrder = new
            {
                CustomerId = customerId,
                ContactId = contactId,
                Reference = $"{_testOrderData.ReferenceAtCustomer}_SO_TEST_{uniqueTestId}",
                Description = $"{_testOrderData.Description}_SO_TEST_{uniqueTestId}",
                DeliveryDate = _testOrderData.Delivery,
                ShipmentMethod = _testOrderData.ShipmentMethod,
                OrderQuantity = _testOrderData.OrderQuantity,
                UnitPrice = _testOrderData.UnitPrice
            };

            var modifiedJson = System.Text.Json.JsonSerializer.Serialize(modifiedSalesOrder, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            Logger.LogInformation("Creating sales order with payload: {SalesOrderJson}", modifiedJson);

            // Act
            var salesOrderResponse = await WithTimeoutAsync(
                CermApiClient.CreateSalesOrderWithJsonAsync(customerId, contactId, modifiedJson),
                TimeSpan.FromSeconds(30),
                "CreateSalesOrderWithJsonAsync"
            );

            // Assert
            salesOrderResponse.Should().NotBeNull("Sales order creation should return a response");
            salesOrderResponse.Success.Should().BeTrue("Sales order creation should succeed");
            salesOrderResponse.SalesOrderId.Should().NotBeNullOrEmpty("Sales order ID should not be empty");

            Logger.LogInformation("Sales order created successfully: SalesOrderId={SalesOrderId}", salesOrderResponse.SalesOrderId);

            LogTestComplete(nameof(CreateSalesOrderWithJsonAsync_WithOrderData_ReturnsValidSalesOrderId), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Sales order creation test failed: {Message}", ex.Message);
            LogTestComplete(nameof(CreateSalesOrderWithJsonAsync_WithOrderData_ReturnsValidSalesOrderId), false);
            throw;
        }
    }

    [Fact]
    public async Task SalesOrderCreation_WithCompleteOrderData_ContainsAllRequiredFields()
    {
        // Arrange
        LogTestStart(nameof(SalesOrderCreation_WithCompleteOrderData_ContainsAllRequiredFields));
        var uniqueTestId = GenerateTestId();
        var customerId = TestDataProvider.GetTestCustomerId();
        var contactId = "1";

        try
        {
            // Create comprehensive sales order with all F003ADB6G8.json data
            var completeSalesOrder = new
            {
                CustomerId = customerId,
                ContactId = contactId,
                Reference = $"{_testOrderData.ReferenceAtCustomer}_COMPLETE_{uniqueTestId}",
                Description = $"{_testOrderData.Description}_COMPLETE_{uniqueTestId}",
                DeliveryDate = _testOrderData.Delivery,
                ShipmentMethod = _testOrderData.ShipmentMethod,
                OrderQuantity = _testOrderData.OrderQuantity,
                UnitPrice = _testOrderData.UnitPrice,
                
                // Address information
                DeliveryAddress = new
                {
                    Name = _testOrderData.Name,
                    Street = _testOrderData.Street,
                    PostalCode = _testOrderData.PostalCode,
                    City = _testOrderData.City,
                    Country = _testOrderData.Country
                },
                
                // Contact information
                Contact = new
                {
                    FirstName = _testOrderData.Contacts.FirstOrDefault()?.FirstName ?? "Guillaume",
                    LastName = _testOrderData.Contacts.FirstOrDefault()?.LastName ?? "Allard",
                    Email = _testOrderData.Contacts.FirstOrDefault()?.Email ?? "DWD@drukwerkdeal.nl",
                    Phone = _testOrderData.Contacts.FirstOrDefault()?.PhoneNumber ?? "+32 485 40 00 96"
                },
                
                // Product specifications
                ProductSpecifications = new
                {
                    Width = _testOrderData.Width,
                    Height = _testOrderData.Height,
                    Shape = _testOrderData.Shape,
                    Substrate = _testOrderData.Substrate,
                    Adhesive = _testOrderData.Adhesive,
                    Winding = _testOrderData.Winding
                }
            };

            var salesOrderJson = System.Text.Json.JsonSerializer.Serialize(completeSalesOrder, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            Logger.LogInformation("Creating complete sales order with payload: {SalesOrderJson}", salesOrderJson);

            // Act
            var salesOrderResponse = await WithTimeoutAsync(
                CermApiClient.CreateSalesOrderWithJsonAsync(customerId, contactId, salesOrderJson),
                TimeSpan.FromSeconds(30),
                "CreateSalesOrderWithJsonAsync (complete)"
            );

            // Assert
            salesOrderResponse.Should().NotBeNull("Complete sales order should be created");
            salesOrderResponse.Success.Should().BeTrue("Sales order creation should succeed");
            salesOrderResponse.SalesOrderId.Should().NotBeNullOrEmpty("Sales order ID should not be empty");

            Logger.LogInformation("Complete sales order created: SalesOrderId={SalesOrderId}", salesOrderResponse.SalesOrderId);
            Logger.LogInformation("Order details verified: Quantity={Quantity}, UnitPrice={UnitPrice}, DeliveryDate={DeliveryDate}", 
                _testOrderData.OrderQuantity, _testOrderData.UnitPrice, _testOrderData.Delivery);

            LogTestComplete(nameof(SalesOrderCreation_WithCompleteOrderData_ContainsAllRequiredFields), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Complete sales order creation test failed: {Message}", ex.Message);
            LogTestComplete(nameof(SalesOrderCreation_WithCompleteOrderData_ContainsAllRequiredFields), false);
            throw;
        }
    }

    [Fact]
    public async Task SalesOrderCreation_WithDifferentCustomerAndContact_WorksCorrectly()
    {
        // Arrange
        LogTestStart(nameof(SalesOrderCreation_WithDifferentCustomerAndContact_WorksCorrectly));
        var uniqueTestId = GenerateTestId();
        var customerId = TestDataProvider.GetTestCustomerId();
        var contactId = "2"; // Different contact ID for testing

        try
        {
            var salesOrderData = new
            {
                CustomerId = customerId,
                ContactId = contactId,
                Reference = $"{_testOrderData.ReferenceAtCustomer}_CONTACT_TEST_{uniqueTestId}",
                Description = $"{_testOrderData.Description}_CONTACT_TEST_{uniqueTestId}",
                DeliveryDate = _testOrderData.Delivery,
                ShipmentMethod = _testOrderData.ShipmentMethod,
                OrderQuantity = _testOrderData.OrderQuantity,
                UnitPrice = _testOrderData.UnitPrice
            };

            var salesOrderJson = System.Text.Json.JsonSerializer.Serialize(salesOrderData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            Logger.LogInformation("Creating sales order with different contact: CustomerId={CustomerId}, ContactId={ContactId}", 
                customerId, contactId);

            // Act
            var salesOrderResponse = await WithTimeoutAsync(
                CermApiClient.CreateSalesOrderWithJsonAsync(customerId, contactId, salesOrderJson),
                TimeSpan.FromSeconds(30),
                "CreateSalesOrderWithJsonAsync (different contact)"
            );

            // Assert
            salesOrderResponse.Should().NotBeNull("Sales order with different contact should be created");
            salesOrderResponse.Success.Should().BeTrue("Sales order creation should succeed");
            salesOrderResponse.SalesOrderId.Should().NotBeNullOrEmpty("Sales order ID should not be empty");

            Logger.LogInformation("Sales order with different contact created: SalesOrderId={SalesOrderId}", salesOrderResponse.SalesOrderId);

            LogTestComplete(nameof(SalesOrderCreation_WithDifferentCustomerAndContact_WorksCorrectly), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Sales order with different contact test failed: {Message}", ex.Message);
            LogTestComplete(nameof(SalesOrderCreation_WithDifferentCustomerAndContact_WorksCorrectly), false);
            throw;
        }
    }

    [Fact]
    public async Task SalesOrderOperations_PerformanceTest_CompletesWithinTimeout()
    {
        // Arrange
        LogTestStart(nameof(SalesOrderOperations_PerformanceTest_CompletesWithinTimeout));
        var uniqueTestId = GenerateTestId();
        var customerId = TestDataProvider.GetTestCustomerId();
        var contactId = "1";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create sales order for performance testing
            var salesOrderData = new
            {
                CustomerId = customerId,
                ContactId = contactId,
                Reference = $"{_testOrderData.ReferenceAtCustomer}_PERF_{uniqueTestId}",
                Description = $"{_testOrderData.Description}_PERF_{uniqueTestId}",
                DeliveryDate = _testOrderData.Delivery,
                ShipmentMethod = _testOrderData.ShipmentMethod,
                OrderQuantity = _testOrderData.OrderQuantity,
                UnitPrice = _testOrderData.UnitPrice
            };

            var salesOrderJson = System.Text.Json.JsonSerializer.Serialize(salesOrderData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var createStartTime = stopwatch.ElapsedMilliseconds;
            var salesOrderResponse = await WithTimeoutAsync(
                CermApiClient.CreateSalesOrderWithJsonAsync(customerId, contactId, salesOrderJson),
                TimeSpan.FromSeconds(15), // Stricter timeout for performance test
                "CreateSalesOrderWithJsonAsync (performance test)"
            );
            var createTime = stopwatch.ElapsedMilliseconds - createStartTime;

            // Assert
            salesOrderResponse.Should().NotBeNull("Sales order should be created within performance timeout");
            salesOrderResponse.Success.Should().BeTrue("Sales order creation should succeed");
            Logger.LogInformation("Sales order creation performance: {CreateTime}ms", createTime);
            createTime.Should().BeLessThan(15000, "Sales order creation should complete within 15 seconds");

            stopwatch.Stop();
            Logger.LogInformation("Total sales order operations performance: {TotalTime}ms", 
                stopwatch.ElapsedMilliseconds);

            LogTestComplete(nameof(SalesOrderOperations_PerformanceTest_CompletesWithinTimeout), true);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(ex, "Sales order performance test failed after {ElapsedMs}ms: {Message}", 
                stopwatch.ElapsedMilliseconds, ex.Message);
            LogTestComplete(nameof(SalesOrderOperations_PerformanceTest_CompletesWithinTimeout), false);
            throw;
        }
    }

    [Fact]
    public async Task SalesOrderValidation_WithMissingRequiredFields_HandlesErrorsGracefully()
    {
        // Arrange
        LogTestStart(nameof(SalesOrderValidation_WithMissingRequiredFields_HandlesErrorsGracefully));
        var customerId = TestDataProvider.GetTestCustomerId();
        var contactId = "1";

        try
        {
            // Create sales order with missing required fields
            var incompleteSalesOrder = new
            {
                CustomerId = customerId,
                ContactId = contactId,
                // Missing Reference, Description, etc.
                OrderQuantity = _testOrderData.OrderQuantity
            };

            var salesOrderJson = System.Text.Json.JsonSerializer.Serialize(incompleteSalesOrder, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            Logger.LogInformation("Testing sales order with missing fields: {SalesOrderJson}", salesOrderJson);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await WithTimeoutAsync(
                    CermApiClient.CreateSalesOrderWithJsonAsync(customerId, contactId, salesOrderJson),
                    TimeSpan.FromSeconds(30),
                    "CreateSalesOrderWithJsonAsync (validation test)"
                );
            });

            Logger.LogInformation("Sales order validation correctly rejected incomplete data: {ExceptionMessage}", 
                exception.Message);

            LogTestComplete(nameof(SalesOrderValidation_WithMissingRequiredFields_HandlesErrorsGracefully), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Sales order validation test failed: {Message}", ex.Message);
            LogTestComplete(nameof(SalesOrderValidation_WithMissingRequiredFields_HandlesErrorsGracefully), false);
            throw;
        }
    }
}
