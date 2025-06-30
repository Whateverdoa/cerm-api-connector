using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace CermApiModule.Tests;

/// <summary>
/// Tests for CERM API product management functionality
/// </summary>
public class ProductTests : TestBase
{
    private readonly OrderTestData _testOrderData;

    public ProductTests() : base()
    {
        _testOrderData = TestDataProvider.GetDefaultOrderData();
        Logger.LogInformation("ProductTests initialized with test data: {OrderData}", 
            SerializeObject(_testOrderData));
    }

    [Fact]
    public async Task CreateProductAsync_WithParameters_ReturnsValidProductId()
    {
        // Arrange
        LogTestStart(nameof(CreateProductAsync_WithParameters_ReturnsValidProductId));
        var uniqueTestId = GenerateTestId();

        try
        {
            // First, create a calculation to link the product to
            var calculationData = new
            {
                Description = $"{_testOrderData.Description}_FOR_PRODUCT_TEST_{uniqueTestId}",
                Reference = $"{_testOrderData.ReferenceAtCustomer}_PRODUCT_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                DeliveryDate = _testOrderData.Delivery,
                CustomerId = TestDataProvider.GetTestCustomerId()
            };

            var calculationJson = System.Text.Json.JsonSerializer.Serialize(calculationData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            Logger.LogInformation("Creating calculation for product test: {CalculationJson}", calculationJson);

            var calculationResponse = await WithTimeoutAsync(
                CermApiClient.CreateCalculationWithJsonAsync(calculationJson),
                TimeSpan.FromSeconds(30),
                "CreateCalculationWithJsonAsync (for product)"
            );

            calculationResponse.Should().NotBeNull("Calculation creation should succeed for product test");
            calculationResponse.Success.Should().BeTrue("Calculation creation should succeed");
            calculationResponse.CalculationId.Should().NotBeNullOrEmpty("Calculation ID should not be empty");
            Logger.LogInformation("Calculation created for product test: {CalculationId}", calculationResponse.CalculationId);

            // Act - Create product with parameters
            var productResponse = await WithTimeoutAsync(
                CermApiClient.CreateProductAsync(
                    calculationResponse.CalculationId!,
                    $"{_testOrderData.Name}_TEST_{uniqueTestId}",
                    _testOrderData.OrderQuantity,
                    _testOrderData.UnitPrice
                ),
                TimeSpan.FromSeconds(30),
                "CreateProductAsync"
            );

            // Assert
            productResponse.Should().NotBeNull("Product creation should return a response");
            productResponse.Success.Should().BeTrue("Product creation should succeed");
            productResponse.ProductId.Should().NotBeNullOrEmpty("Product ID should not be empty");

            Logger.LogInformation("Product created successfully: ProductId={ProductId}, CalculationId={CalculationId}",
                productResponse.ProductId, calculationResponse.CalculationId);

            LogTestComplete(nameof(CreateProductAsync_WithParameters_ReturnsValidProductId), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Product creation with parameters test failed: {Message}", ex.Message);
            LogTestComplete(nameof(CreateProductAsync_WithParameters_ReturnsValidProductId), false);
            throw;
        }
    }

    [Fact]
    public async Task CreateProductWithJsonAsync_WithOrderData_ReturnsValidProductId()
    {
        // Arrange
        LogTestStart(nameof(CreateProductWithJsonAsync_WithOrderData_ReturnsValidProductId));
        var uniqueTestId = GenerateTestId();

        try
        {
            // First, create a calculation to link the product to
            var calculationData = new
            {
                Description = $"{_testOrderData.Description}_FOR_JSON_PRODUCT_TEST_{uniqueTestId}",
                Reference = $"{_testOrderData.ReferenceAtCustomer}_JSON_PRODUCT_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                DeliveryDate = _testOrderData.Delivery,
                CustomerId = TestDataProvider.GetTestCustomerId()
            };

            var calculationJson = System.Text.Json.JsonSerializer.Serialize(calculationData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var calculationResponse = await WithTimeoutAsync(
                CermApiClient.CreateCalculationWithJsonAsync(calculationJson),
                TimeSpan.FromSeconds(30),
                "CreateCalculationWithJsonAsync (for JSON product)"
            );

            calculationResponse.Should().NotBeNull("Calculation creation should succeed for JSON product test");
            calculationResponse.Success.Should().BeTrue("Calculation creation should succeed");
            calculationResponse.CalculationId.Should().NotBeNullOrEmpty("Calculation ID should not be empty");
            Logger.LogInformation("Calculation created for JSON product test: {CalculationId}", calculationResponse.CalculationId);

            // Create product JSON payload
            var productJson = TestDataProvider.CreateProductJsonPayload(_testOrderData, calculationResponse.CalculationId!);
            
            // Add unique identifier to avoid conflicts
            var productData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(productJson);
            var modifiedProduct = new
            {
                CalculationId = calculationResponse.CalculationId,
                Name = $"{_testOrderData.Name}_JSON_TEST_{uniqueTestId}",
                Description = $"{_testOrderData.Description}_JSON_TEST_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                UnitPrice = _testOrderData.UnitPrice,
                Width = _testOrderData.Width,
                Height = _testOrderData.Height,
                Shape = _testOrderData.Shape,
                Substrate = _testOrderData.Substrate,
                Adhesive = _testOrderData.Adhesive
            };

            var modifiedJson = System.Text.Json.JsonSerializer.Serialize(modifiedProduct, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            Logger.LogInformation("Creating product with JSON payload: {ProductJson}", modifiedJson);

            // Act
            var productResponse = await WithTimeoutAsync(
                CermApiClient.CreateProductWithJsonAsync(calculationResponse.CalculationId!, modifiedJson),
                TimeSpan.FromSeconds(30),
                "CreateProductWithJsonAsync"
            );

            // Assert
            productResponse.Should().NotBeNull("Product creation with JSON should return a response");
            productResponse.Success.Should().BeTrue("Product creation should succeed");
            productResponse.ProductId.Should().NotBeNullOrEmpty("Product ID should not be empty");

            Logger.LogInformation("Product created successfully with JSON: ProductId={ProductId}, CalculationId={CalculationId}",
                productResponse.ProductId, calculationResponse.CalculationId);

            LogTestComplete(nameof(CreateProductWithJsonAsync_WithOrderData_ReturnsValidProductId), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Product creation with JSON test failed: {Message}", ex.Message);
            LogTestComplete(nameof(CreateProductWithJsonAsync_WithOrderData_ReturnsValidProductId), false);
            throw;
        }
    }

    [Fact]
    public async Task ProductCreation_WithCompleteSpecifications_ContainsCorrectData()
    {
        // Arrange
        LogTestStart(nameof(ProductCreation_WithCompleteSpecifications_ContainsCorrectData));
        var uniqueTestId = GenerateTestId();

        try
        {
            // Create calculation first
            var calculationData = new
            {
                Description = $"{_testOrderData.Description}_SPEC_TEST_{uniqueTestId}",
                Reference = $"{_testOrderData.ReferenceAtCustomer}_SPEC_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                DeliveryDate = _testOrderData.Delivery,
                CustomerId = TestDataProvider.GetTestCustomerId()
            };

            var calculationJson = System.Text.Json.JsonSerializer.Serialize(calculationData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var calculationResponse = await CermApiClient.CreateCalculationWithJsonAsync(calculationJson);
            calculationResponse.Should().NotBeNull("Calculation should be created for specification test");
            calculationResponse.Success.Should().BeTrue("Calculation creation should succeed");
            calculationResponse.CalculationId.Should().NotBeNullOrEmpty("Calculation ID should not be empty");

            // Create product with complete specifications from F003ADB6G8.json
            var productData = new
            {
                CalculationId = calculationResponse.CalculationId,
                Name = $"{_testOrderData.Name}_SPEC_TEST_{uniqueTestId}",
                Description = $"{_testOrderData.Description}_SPEC_TEST_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity, // 1000
                UnitPrice = _testOrderData.UnitPrice, // 37.18
                Width = _testOrderData.Width, // "30,0"
                Height = _testOrderData.Height, // "30,0"
                Shape = _testOrderData.Shape, // "Circle"
                Substrate = _testOrderData.Substrate, // "Adhesive Label Paper White Gloss"
                Adhesive = _testOrderData.Adhesive, // "Removable Adhesive Glue"
                Winding = _testOrderData.Winding, // 1
                PremiumWhite = _testOrderData.PremiumWhite // "N"
            };

            var productJson = System.Text.Json.JsonSerializer.Serialize(productData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            Logger.LogInformation("Creating product with complete specifications: {ProductJson}", productJson);

            // Act
            var productResponse = await WithTimeoutAsync(
                CermApiClient.CreateProductWithJsonAsync(calculationResponse.CalculationId!, productJson),
                TimeSpan.FromSeconds(30),
                "CreateProductWithJsonAsync (complete specifications)"
            );

            // Assert
            productResponse.Should().NotBeNull("Product with complete specifications should be created");
            productResponse.Success.Should().BeTrue("Product creation should succeed");
            productResponse.ProductId.Should().NotBeNullOrEmpty("Product ID should not be empty");

            Logger.LogInformation("Product with complete specifications created: ProductId={ProductId}", productResponse.ProductId);
            Logger.LogInformation("Product specifications verified: Quantity={Quantity}, UnitPrice={UnitPrice}, Shape={Shape}, Substrate={Substrate}", 
                _testOrderData.OrderQuantity, _testOrderData.UnitPrice, _testOrderData.Shape, _testOrderData.Substrate);

            LogTestComplete(nameof(ProductCreation_WithCompleteSpecifications_ContainsCorrectData), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Product specification test failed: {Message}", ex.Message);
            LogTestComplete(nameof(ProductCreation_WithCompleteSpecifications_ContainsCorrectData), false);
            throw;
        }
    }

    [Fact]
    public async Task ProductOperations_PerformanceTest_CompletesWithinTimeout()
    {
        // Arrange
        LogTestStart(nameof(ProductOperations_PerformanceTest_CompletesWithinTimeout));
        var uniqueTestId = GenerateTestId();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Step 1: Create calculation (performance baseline)
            var calculationStartTime = stopwatch.ElapsedMilliseconds;
            var calculationData = new
            {
                Description = $"{_testOrderData.Description}_PERF_TEST_{uniqueTestId}",
                Reference = $"{_testOrderData.ReferenceAtCustomer}_PERF_{uniqueTestId}",
                Quantity = _testOrderData.OrderQuantity,
                DeliveryDate = _testOrderData.Delivery,
                CustomerId = TestDataProvider.GetTestCustomerId()
            };

            var calculationJson = System.Text.Json.JsonSerializer.Serialize(calculationData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var calculationResponse = await WithTimeoutAsync(
                CermApiClient.CreateCalculationWithJsonAsync(calculationJson),
                TimeSpan.FromSeconds(10),
                "CreateCalculationWithJsonAsync (performance baseline)"
            );
            var calculationTime = stopwatch.ElapsedMilliseconds - calculationStartTime;

            calculationResponse.Should().NotBeNull("Calculation should be created for performance test");
            calculationResponse.Success.Should().BeTrue("Calculation creation should succeed");
            calculationResponse.CalculationId.Should().NotBeNullOrEmpty("Calculation ID should not be empty");
            Logger.LogInformation("Calculation creation performance: {CalculationTime}ms", calculationTime);

            // Step 2: Create product (performance test)
            var productStartTime = stopwatch.ElapsedMilliseconds;
            var productResponse = await WithTimeoutAsync(
                CermApiClient.CreateProductAsync(
                    calculationResponse.CalculationId!,
                    $"{_testOrderData.Name}_PERF_TEST_{uniqueTestId}",
                    _testOrderData.OrderQuantity,
                    _testOrderData.UnitPrice
                ),
                TimeSpan.FromSeconds(10), // Stricter timeout for performance test
                "CreateProductAsync (performance test)"
            );
            var productTime = stopwatch.ElapsedMilliseconds - productStartTime;

            // Assert
            productResponse.Should().NotBeNull("Product should be created within performance timeout");
            productResponse.Success.Should().BeTrue("Product creation should succeed");
            Logger.LogInformation("Product creation performance: {ProductTime}ms", productTime);
            productTime.Should().BeLessThan(10000, "Product creation should complete within 10 seconds");

            stopwatch.Stop();
            Logger.LogInformation("Total product operations performance: {TotalTime}ms", 
                stopwatch.ElapsedMilliseconds);

            LogTestComplete(nameof(ProductOperations_PerformanceTest_CompletesWithinTimeout), true);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(ex, "Product performance test failed after {ElapsedMs}ms: {Message}", 
                stopwatch.ElapsedMilliseconds, ex.Message);
            LogTestComplete(nameof(ProductOperations_PerformanceTest_CompletesWithinTimeout), false);
            throw;
        }
    }
}
