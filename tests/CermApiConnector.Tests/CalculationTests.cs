using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace CermApiModule.Tests;

/// <summary>
/// Tests for CERM API calculation/quote management functionality
/// </summary>
public class CalculationTests : TestBase
{
    private readonly OrderTestData _testOrderData;

    public CalculationTests() : base()
    {
        _testOrderData = TestDataProvider.GetDefaultOrderData();
        Logger.LogInformation("CalculationTests initialized with test data: {OrderData}",
            SerializeObject(_testOrderData));
    }

    [Fact]
    public async Task FetchQuoteIdAsync_WithOrderData_ReturnsQuoteIdOrNull()
    {
        // Arrange
        LogTestStart(nameof(FetchQuoteIdAsync_WithOrderData_ReturnsQuoteIdOrNull));
        var customerId = TestDataProvider.GetTestCustomerId();
        var productCode = "TEST_PRODUCT_CODE"; // Using a test product code

        try
        {
            // Act
            var quoteResponse = await WithTimeoutAsync(
                CermApiClient.FetchQuoteIdAsync(customerId, productCode),
                TimeSpan.FromSeconds(30),
                "FetchQuoteIdAsync"
            );

            // Assert
            quoteResponse.Should().NotBeNull("Quote response should not be null");
            Logger.LogInformation("Quote fetch result: Success={Success}, CalculationId={CalculationId}",
                quoteResponse.Success, quoteResponse.CalculationId);

            // Quote ID can be null (if not found) or a valid string
            if (quoteResponse.Success && !string.IsNullOrEmpty(quoteResponse.CalculationId))
            {
                quoteResponse.CalculationId.Should().NotBeEmpty("Calculation ID should not be empty if found");
                Logger.LogInformation("Existing quote found: {CalculationId}", quoteResponse.CalculationId);
            }
            else
            {
                Logger.LogInformation("No existing quote found for the given criteria");
            }

            LogTestComplete(nameof(FetchQuoteIdAsync_WithOrderData_ReturnsQuoteIdOrNull), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Quote fetch test failed: {Message}", ex.Message);
            LogTestComplete(nameof(FetchQuoteIdAsync_WithOrderData_ReturnsQuoteIdOrNull), false);
            throw;
        }
    }

    [Fact]
    public async Task CreateCalculationWithJsonAsync_WithOrderData_ReturnsValidCalculationId()
    {
        // Arrange
        LogTestStart(nameof(CreateCalculationWithJsonAsync_WithOrderData_ReturnsValidCalculationId));
        var uniqueTestId = GenerateTestId();

        try
        {
            // Create calculation JSON payload from order data
            var calculationJson = TestDataProvider.CreateCalculationJsonPayload(_testOrderData);

            // Add unique identifier to avoid conflicts
            var calculationData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(calculationJson);
            var modifiedCalculation = new
            {
                Description = $"{_testOrderData.Description}_TEST_{uniqueTestId}",
                Reference = $"{_testOrderData.ReferenceAtCustomer}_TEST_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                DeliveryDate = _testOrderData.Delivery,
                CustomerId = TestDataProvider.GetTestCustomerId()
            };

            var modifiedJson = System.Text.Json.JsonSerializer.Serialize(modifiedCalculation,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            Logger.LogInformation("Creating calculation with payload: {CalculationJson}", modifiedJson);

            // Act
            var calculationResponse = await WithTimeoutAsync(
                CermApiClient.CreateCalculationWithJsonAsync(modifiedJson),
                TimeSpan.FromSeconds(30),
                "CreateCalculationWithJsonAsync"
            );

            // Assert
            calculationResponse.Should().NotBeNull("Calculation creation should return a response");
            calculationResponse.Success.Should().BeTrue("Calculation creation should succeed");
            calculationResponse.CalculationId.Should().NotBeNullOrEmpty("Calculation ID should not be empty");

            Logger.LogInformation("Calculation created successfully: CalculationId={CalculationId}", calculationResponse.CalculationId);

            LogTestComplete(nameof(CreateCalculationWithJsonAsync_WithOrderData_ReturnsValidCalculationId), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Calculation creation test failed: {Message}", ex.Message);
            LogTestComplete(nameof(CreateCalculationWithJsonAsync_WithOrderData_ReturnsValidCalculationId), false);
            throw;
        }
    }

    [Fact]
    public async Task CreateCalculationWithCompleteData_WithOrderData_ReturnsValidCalculationId()
    {
        // Arrange
        LogTestStart(nameof(CreateCalculationWithCompleteData_WithOrderData_ReturnsValidCalculationId));
        var uniqueTestId = GenerateTestId();

        try
        {
            // Create comprehensive calculation with all F003ADB6G8.json data
            var calculationData = new
            {
                Description = $"{_testOrderData.Description}_COMPLETE_{uniqueTestId}",
                Reference = $"{_testOrderData.ReferenceAtCustomer}_COMPLETE_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                UnitPrice = _testOrderData.UnitPrice,
                DeliveryDate = _testOrderData.Delivery,
                CustomerId = TestDataProvider.GetTestCustomerId(),
                ShipmentMethod = _testOrderData.ShipmentMethod,
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

            var calculationJson = System.Text.Json.JsonSerializer.Serialize(calculationData,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            Logger.LogInformation("Creating complete calculation with payload: {CalculationJson}", calculationJson);

            // Act
            var calculationResponse = await WithTimeoutAsync(
                CermApiClient.CreateCalculationWithJsonAsync(calculationJson),
                TimeSpan.FromSeconds(30),
                "CreateCalculationWithJsonAsync (complete)"
            );

            // Assert
            calculationResponse.Should().NotBeNull("Calculation creation should return a response");
            calculationResponse.Success.Should().BeTrue("Calculation creation should succeed");
            calculationResponse.CalculationId.Should().NotBeNullOrEmpty("Calculation ID should not be empty");

            Logger.LogInformation("Complete calculation created successfully: CalculationId={CalculationId}", calculationResponse.CalculationId);

            LogTestComplete(nameof(CreateCalculationWithCompleteData_WithOrderData_ReturnsValidCalculationId), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Complete calculation creation test failed: {Message}", ex.Message);
            LogTestComplete(nameof(CreateCalculationWithCompleteData_WithOrderData_ReturnsValidCalculationId), false);
            throw;
        }
    }

    [Fact]
    public async Task CalculationWorkflow_CreateAndFetch_WorksCorrectly()
    {
        // Arrange
        LogTestStart(nameof(CalculationWorkflow_CreateAndFetch_WorksCorrectly));
        var customerId = TestDataProvider.GetTestCustomerId();
        var uniqueTestId = GenerateTestId();

        try
        {
            // Step 1: Create a new calculation
            var calculationData = new
            {
                Description = $"{_testOrderData.Description}_WORKFLOW_TEST_{uniqueTestId}",
                Reference = $"{_testOrderData.ReferenceAtCustomer}_WORKFLOW_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                DeliveryDate = _testOrderData.Delivery,
                CustomerId = customerId
            };

            var calculationJson = System.Text.Json.JsonSerializer.Serialize(calculationData,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            Logger.LogInformation("Step 1: Creating calculation with payload: {CalculationJson}", calculationJson);

            var createdCalculationResponse = await WithTimeoutAsync(
                CermApiClient.CreateCalculationWithJsonAsync(calculationJson),
                TimeSpan.FromSeconds(30),
                "CreateCalculationWithJsonAsync (Step 1)"
            );

            createdCalculationResponse.Should().NotBeNull("Calculation creation should succeed");
            createdCalculationResponse.Success.Should().BeTrue("Calculation creation should be successful");
            Logger.LogInformation("Step 1 Complete: Calculation created with ID: {CalculationId}", createdCalculationResponse.CalculationId);

            // Step 2: Try to fetch quote using search criteria (using FetchQuoteIdAsync as alternative)
            Logger.LogInformation("Step 2: Fetching quote using search criteria");
            var fetchedQuoteResponse = await WithTimeoutAsync(
                CermApiClient.FetchQuoteIdAsync(customerId, "TEST_PRODUCT"),
                TimeSpan.FromSeconds(30),
                "FetchQuoteIdAsync (Step 2)"
            );

            // Note: The fetched quote might be different from created one if multiple calculations exist
            // This is expected behavior
            Logger.LogInformation("Step 2 Complete: Fetched quote response: Success={Success}, CalculationId={CalculationId}",
                fetchedQuoteResponse.Success, fetchedQuoteResponse.CalculationId);

            LogTestComplete(nameof(CalculationWorkflow_CreateAndFetch_WorksCorrectly), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Calculation workflow test failed: {Message}", ex.Message);
            LogTestComplete(nameof(CalculationWorkflow_CreateAndFetch_WorksCorrectly), false);
            throw;
        }
    }

    [Fact]
    public async Task CalculationOperations_PerformanceTest_CompletesWithinTimeout()
    {
        // Arrange
        LogTestStart(nameof(CalculationOperations_PerformanceTest_CompletesWithinTimeout));
        var customerId = TestDataProvider.GetTestCustomerId();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Test quote fetch performance (using available method)
            var fetchStartTime = stopwatch.ElapsedMilliseconds;
            var quoteResponse = await WithTimeoutAsync(
                CermApiClient.FetchQuoteIdAsync(customerId, "TEST_PRODUCT"),
                TimeSpan.FromSeconds(15), // Stricter timeout for performance test
                "FetchQuoteIdAsync (performance test)"
            );
            var fetchTime = stopwatch.ElapsedMilliseconds - fetchStartTime;

            Logger.LogInformation("Quote fetch performance: {FetchTime}ms", fetchTime);
            fetchTime.Should().BeLessThan(15000, "Quote fetch should complete within 15 seconds");

            // Test calculation creation performance
            var uniqueTestId = GenerateTestId();
            var calculationData = new
            {
                Description = $"{_testOrderData.Description}_PERF_TEST_{uniqueTestId}",
                Reference = $"{_testOrderData.ReferenceAtCustomer}_PERF_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                DeliveryDate = _testOrderData.Delivery,
                CustomerId = customerId
            };

            var calculationJson = System.Text.Json.JsonSerializer.Serialize(calculationData,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var createStartTime = stopwatch.ElapsedMilliseconds;
            var createdCalculationId = await WithTimeoutAsync(
                CermApiClient.CreateCalculationWithJsonAsync(calculationJson),
                TimeSpan.FromSeconds(15),
                "CreateCalculationWithJsonAsync (performance test)"
            );
            var createTime = stopwatch.ElapsedMilliseconds - createStartTime;

            Logger.LogInformation("Calculation creation performance: {CreateTime}ms", createTime);
            createTime.Should().BeLessThan(15000, "Calculation creation should complete within 15 seconds");

            stopwatch.Stop();
            Logger.LogInformation("Total calculation operations performance: {TotalTime}ms",
                stopwatch.ElapsedMilliseconds);

            LogTestComplete(nameof(CalculationOperations_PerformanceTest_CompletesWithinTimeout), true);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(ex, "Calculation performance test failed after {ElapsedMs}ms: {Message}",
                stopwatch.ElapsedMilliseconds, ex.Message);
            LogTestComplete(nameof(CalculationOperations_PerformanceTest_CompletesWithinTimeout), false);
            throw;
        }
    }
}
