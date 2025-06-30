using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace CermApiModule.Tests;

/// <summary>
/// Integration tests for complete CERM API workflows
/// </summary>
public class IntegrationTests : TestBase
{
    private readonly OrderTestData _testOrderData;

    public IntegrationTests() : base()
    {
        _testOrderData = TestDataProvider.GetDefaultOrderData();
        Logger.LogInformation("IntegrationTests initialized with test data: {OrderData}", 
            SerializeObject(_testOrderData));
    }

    [Fact]
    public async Task CompleteOrderWorkflow_EndToEnd_CreatesAllEntitiesSuccessfully()
    {
        // Arrange
        LogTestStart(nameof(CompleteOrderWorkflow_EndToEnd_CreatesAllEntitiesSuccessfully));
        var uniqueTestId = GenerateTestId();
        var customerId = TestDataProvider.GetTestCustomerId();
        var contactId = "1";
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        string? addressId = null;
        string? calculationId = null;
        string? productId = null;
        string? salesOrderId = null;

        try
        {
            // Step 1: Authentication (already handled by TestBase)
            Logger.LogInformation("Step 1: Authentication - ✅ Already authenticated");

            // Step 2: Create/Fetch Address
            Logger.LogInformation("Step 2: Creating address from order data");
            var addressJson = TestDataProvider.CreateAddressJsonPayload(_testOrderData, customerId);
            
            var step2StartTime = stopwatch.ElapsedMilliseconds;
            var addressResponse = await WithTimeoutAsync(
                CermApiClient.CreateAddressWithJsonAsync(addressJson),
                TimeSpan.FromSeconds(30),
                "CreateAddressWithJsonAsync (Step 2)"
            );
            var step2Time = stopwatch.ElapsedMilliseconds - step2StartTime;

            addressResponse.Should().NotBeNull("Address creation should succeed");
            addressResponse.Success.Should().BeTrue("Address creation should succeed");
            addressResponse.AddressId.Should().NotBeNullOrEmpty("Address ID should not be empty");
            addressId = addressResponse.AddressId;
            Logger.LogInformation("Step 2 Complete: Address created - AddressId={AddressId} ({Time}ms)", 
                addressId, step2Time);

            // Step 3: Create Calculation
            Logger.LogInformation("Step 3: Creating calculation from order data");
            var calculationData = new
            {
                Description = $"{_testOrderData.Description}_INTEGRATION_{uniqueTestId}",
                Reference = $"{_testOrderData.ReferenceAtCustomer}_INTEGRATION_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                DeliveryDate = _testOrderData.Delivery,
                CustomerId = customerId
            };

            var calculationJson = System.Text.Json.JsonSerializer.Serialize(calculationData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var step3StartTime = stopwatch.ElapsedMilliseconds;
            var calculationResponse = await WithTimeoutAsync(
                CermApiClient.CreateCalculationWithJsonAsync(calculationJson),
                TimeSpan.FromSeconds(30),
                "CreateCalculationWithJsonAsync (Step 3)"
            );
            var step3Time = stopwatch.ElapsedMilliseconds - step3StartTime;

            calculationResponse.Should().NotBeNull("Calculation creation should succeed");
            calculationResponse.Success.Should().BeTrue("Calculation creation should succeed");
            calculationResponse.CalculationId.Should().NotBeNullOrEmpty("Calculation ID should not be empty");
            calculationId = calculationResponse.CalculationId;
            Logger.LogInformation("Step 3 Complete: Calculation created - CalculationId={CalculationId} ({Time}ms)",
                calculationId, step3Time);

            // Step 4: Create Product
            Logger.LogInformation("Step 4: Creating product linked to calculation");
            var productData = new
            {
                CalculationId = calculationId,
                Name = $"{_testOrderData.Name}_INTEGRATION_{uniqueTestId}",
                Description = $"{_testOrderData.Description}_INTEGRATION_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                UnitPrice = _testOrderData.UnitPrice,
                Width = _testOrderData.Width,
                Height = _testOrderData.Height,
                Shape = _testOrderData.Shape,
                Substrate = _testOrderData.Substrate,
                Adhesive = _testOrderData.Adhesive
            };

            var productJson = System.Text.Json.JsonSerializer.Serialize(productData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var step4StartTime = stopwatch.ElapsedMilliseconds;
            var productResponse = await WithTimeoutAsync(
                CermApiClient.CreateProductWithJsonAsync(calculationId!, productJson),
                TimeSpan.FromSeconds(30),
                "CreateProductWithJsonAsync (Step 4)"
            );
            var step4Time = stopwatch.ElapsedMilliseconds - step4StartTime;

            productResponse.Should().NotBeNull("Product creation should succeed");
            productResponse.Success.Should().BeTrue("Product creation should succeed");
            productResponse.ProductId.Should().NotBeNullOrEmpty("Product ID should not be empty");
            productId = productResponse.ProductId;
            Logger.LogInformation("Step 4 Complete: Product created - ProductId={ProductId} ({Time}ms)", 
                productId, step4Time);

            // Step 5: Create Sales Order
            Logger.LogInformation("Step 5: Creating sales order with all components");
            var salesOrderData = new
            {
                CustomerId = customerId,
                ContactId = contactId,
                Reference = $"{_testOrderData.ReferenceAtCustomer}_INTEGRATION_{uniqueTestId}",
                Description = $"{_testOrderData.Description}_INTEGRATION_{uniqueTestId}",
                DeliveryDate = _testOrderData.Delivery,
                ShipmentMethod = _testOrderData.ShipmentMethod,
                OrderQuantity = _testOrderData.OrderQuantity,
                UnitPrice = _testOrderData.UnitPrice,
                AddressId = addressId,
                CalculationId = calculationId,
                ProductId = productId
            };

            var salesOrderJson = System.Text.Json.JsonSerializer.Serialize(salesOrderData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var step5StartTime = stopwatch.ElapsedMilliseconds;
            var salesOrderResponse = await WithTimeoutAsync(
                CermApiClient.CreateSalesOrderWithJsonAsync(customerId, contactId, salesOrderJson),
                TimeSpan.FromSeconds(30),
                "CreateSalesOrderWithJsonAsync (Step 5)"
            );
            var step5Time = stopwatch.ElapsedMilliseconds - step5StartTime;

            salesOrderResponse.Should().NotBeNull("Sales order creation should succeed");
            salesOrderResponse.Success.Should().BeTrue("Sales order creation should succeed");
            salesOrderResponse.SalesOrderId.Should().NotBeNullOrEmpty("Sales order ID should not be empty");
            salesOrderId = salesOrderResponse.SalesOrderId;
            Logger.LogInformation("Step 5 Complete: Sales order created - SalesOrderId={SalesOrderId} ({Time}ms)",
                salesOrderId, step5Time);

            stopwatch.Stop();

            // Final Validation
            Logger.LogInformation("=== INTEGRATION TEST COMPLETE ===");
            Logger.LogInformation("Total workflow time: {TotalTime}ms", stopwatch.ElapsedMilliseconds);
            Logger.LogInformation("Created entities:");
            Logger.LogInformation("  - Address ID: {AddressId}", addressId);
            Logger.LogInformation("  - Calculation ID: {CalculationId}", calculationId);
            Logger.LogInformation("  - Product ID: {ProductId}", productId);
            Logger.LogInformation("  - Sales Order ID: {SalesOrderId}", salesOrderId);

            // Performance assertions
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(120000, 
                "Complete workflow should finish within 2 minutes");

            LogTestComplete(nameof(CompleteOrderWorkflow_EndToEnd_CreatesAllEntitiesSuccessfully), true);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(ex, "Integration test failed after {ElapsedMs}ms at step with entities: Address={AddressId}, Calculation={CalculationId}, Product={ProductId}, SalesOrder={SalesOrderId}", 
                stopwatch.ElapsedMilliseconds, addressId, calculationId, productId, salesOrderId);
            LogTestComplete(nameof(CompleteOrderWorkflow_EndToEnd_CreatesAllEntitiesSuccessfully), false);
            throw;
        }
    }

    [Fact]
    public async Task WorkflowErrorHandling_WithInvalidData_HandlesGracefully()
    {
        // Arrange
        LogTestStart(nameof(WorkflowErrorHandling_WithInvalidData_HandlesGracefully));
        var uniqueTestId = GenerateTestId();

        try
        {
            // Test with invalid customer ID
            var invalidCustomerId = "INVALID_CUSTOMER_ID";
            
            Logger.LogInformation("Testing workflow with invalid customer ID: {InvalidCustomerId}", invalidCustomerId);

            // This should fail gracefully
            var exception = await Assert.ThrowsAsync<Exception>(async () =>
            {
                var addressJson = TestDataProvider.CreateAddressJsonPayload(_testOrderData, invalidCustomerId);
                await WithTimeoutAsync(
                    CermApiClient.CreateAddressWithJsonAsync(addressJson),
                    TimeSpan.FromSeconds(30),
                    "CreateAddressWithJsonAsync (invalid customer)"
                );
            });

            Logger.LogInformation("Workflow correctly handled invalid customer ID: {ExceptionMessage}", 
                exception.Message);

            LogTestComplete(nameof(WorkflowErrorHandling_WithInvalidData_HandlesGracefully), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Workflow error handling test failed: {Message}", ex.Message);
            LogTestComplete(nameof(WorkflowErrorHandling_WithInvalidData_HandlesGracefully), false);
            throw;
        }
    }

    [Fact]
    public async Task ConcurrentWorkflows_MultipleOrders_HandleCorrectly()
    {
        // Arrange
        LogTestStart(nameof(ConcurrentWorkflows_MultipleOrders_HandleCorrectly));
        var customerId = TestDataProvider.GetTestCustomerId();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Create multiple concurrent address creation tasks
            var tasks = new List<Task<aws_b2b_mod1.Models.AddressIdResponse>>();
            for (int i = 0; i < 3; i++)
            {
                var uniqueTestId = GenerateTestId();
                var addressJson = TestDataProvider.CreateAddressJsonPayload(_testOrderData, customerId);

                // Modify JSON to make each address unique
                var addressData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(addressJson);
                var modifiedAddress = new
                {
                    CustomerId = customerId,
                    Name = $"{_testOrderData.Name}_CONCURRENT_{uniqueTestId}_{i}",
                    Street = _testOrderData.Street,
                    PostalCode = _testOrderData.PostalCode,
                    City = _testOrderData.City,
                    Country = _testOrderData.Country,
                    IsDeliveryAddress = true,
                    IsInvoiceAddress = false,
                    Active = true
                };

                var modifiedJson = System.Text.Json.JsonSerializer.Serialize(modifiedAddress,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                tasks.Add(CermApiClient.CreateAddressWithJsonAsync(modifiedJson));
            }

            // Wait for all tasks to complete
            var addressResponses = await Task.WhenAll(tasks);
            stopwatch.Stop();

            // Assert
            addressResponses.Should().HaveCount(3, "All concurrent address creations should complete");
            addressResponses.Should().OnlyContain(response => response.Success && !string.IsNullOrEmpty(response.AddressId),
                "All address creations should succeed with valid IDs");

            var addressIds = addressResponses.Select(r => r.AddressId).ToArray();
            Logger.LogInformation("Concurrent workflows completed successfully in {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
            Logger.LogInformation("Created address IDs: {AddressIds}", string.Join(", ", addressIds));

            LogTestComplete(nameof(ConcurrentWorkflows_MultipleOrders_HandleCorrectly), true);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(ex, "Concurrent workflows test failed after {ElapsedMs}ms: {Message}", 
                stopwatch.ElapsedMilliseconds, ex.Message);
            LogTestComplete(nameof(ConcurrentWorkflows_MultipleOrders_HandleCorrectly), false);
            throw;
        }
    }

    [Fact]
    public async Task DataConsistency_AcrossEndpoints_MaintainsIntegrity()
    {
        // Arrange
        LogTestStart(nameof(DataConsistency_AcrossEndpoints_MaintainsIntegrity));
        var uniqueTestId = GenerateTestId();
        var customerId = TestDataProvider.GetTestCustomerId();

        try
        {
            // Create calculation with specific data
            var calculationData = new
            {
                Description = $"{_testOrderData.Description}_CONSISTENCY_{uniqueTestId}",
                Reference = $"{_testOrderData.ReferenceAtCustomer}_CONSISTENCY_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                DeliveryDate = _testOrderData.Delivery,
                CustomerId = customerId
            };

            var calculationJson = System.Text.Json.JsonSerializer.Serialize(calculationData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var calculationResponse = await CermApiClient.CreateCalculationWithJsonAsync(calculationJson);
            calculationResponse.Should().NotBeNull("Calculation should be created for consistency test");
            calculationResponse.Success.Should().BeTrue("Calculation creation should succeed");
            calculationResponse.CalculationId.Should().NotBeNullOrEmpty("Calculation ID should not be empty");

            // Create product linked to calculation with same data
            var productData = new
            {
                CalculationId = calculationResponse.CalculationId,
                Name = $"{_testOrderData.Name}_CONSISTENCY_{uniqueTestId}",
                Description = calculationData.Description, // Same description
                Quantity = calculationData.Quantity, // Same quantity
                UnitPrice = _testOrderData.UnitPrice,
                Width = _testOrderData.Width,
                Height = _testOrderData.Height
            };

            var productJson = System.Text.Json.JsonSerializer.Serialize(productData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var productResponse = await CermApiClient.CreateProductWithJsonAsync(calculationResponse.CalculationId!, productJson);
            productResponse.Should().NotBeNull("Product should be created and linked to calculation");
            productResponse.Success.Should().BeTrue("Product creation should succeed");
            productResponse.ProductId.Should().NotBeNullOrEmpty("Product ID should not be empty");

            Logger.LogInformation("Data consistency verified: CalculationId={CalculationId}, ProductId={ProductId}",
                calculationResponse.CalculationId, productResponse.ProductId);
            Logger.LogInformation("Consistent data: Description={Description}, Quantity={Quantity}", 
                calculationData.Description, calculationData.Quantity);

            LogTestComplete(nameof(DataConsistency_AcrossEndpoints_MaintainsIntegrity), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Data consistency test failed: {Message}", ex.Message);
            LogTestComplete(nameof(DataConsistency_AcrossEndpoints_MaintainsIntegrity), false);
            throw;
        }
    }
}
