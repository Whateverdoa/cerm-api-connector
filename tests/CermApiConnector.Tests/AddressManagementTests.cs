using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace CermApiModule.Tests;

/// <summary>
/// Tests for CERM API address management functionality
/// </summary>
public class AddressManagementTests : TestBase
{
    private readonly OrderTestData _testOrderData;

    public AddressManagementTests() : base()
    {
        _testOrderData = TestDataProvider.GetDefaultOrderData();
        Logger.LogInformation("AddressManagementTests initialized with test data: {OrderData}",
            SerializeObject(_testOrderData));
    }

    [Fact]
    public async Task FetchAddressIdAsync_WithOrderData_ReturnsAddressIdOrNull()
    {
        // Arrange
        LogTestStart(nameof(FetchAddressIdAsync_WithOrderData_ReturnsAddressIdOrNull));
        var customerId = TestDataProvider.GetTestCustomerId();

        try
        {
            // Act
            var addressResponse = await WithTimeoutAsync(
                CermApiClient.FetchAddressIdAsync(
                    customerId,
                    _testOrderData.PostalCode,
                    _testOrderData.Street,
                    _testOrderData.City,
                    _testOrderData.Country
                ),
                TimeSpan.FromSeconds(30),
                "FetchAddressIdAsync"
            );

            // Assert
            addressResponse.Should().NotBeNull("Address response should not be null");
            Logger.LogInformation("Address fetch result: Success={Success}, AddressId={AddressId}",
                addressResponse.Success, addressResponse.AddressId);

            // Address ID can be null (if not found) or a valid string
            if (addressResponse.Success && !string.IsNullOrEmpty(addressResponse.AddressId))
            {
                addressResponse.AddressId.Should().NotBeEmpty("Address ID should not be empty if found");
                Logger.LogInformation("Existing address found: {AddressId}", addressResponse.AddressId);
            }
            else
            {
                Logger.LogInformation("No existing address found for the given criteria");
            }

            LogTestComplete(nameof(FetchAddressIdAsync_WithOrderData_ReturnsAddressIdOrNull), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Address fetch test failed: {Message}", ex.Message);
            LogTestComplete(nameof(FetchAddressIdAsync_WithOrderData_ReturnsAddressIdOrNull), false);
            throw;
        }
    }

    [Fact]
    public async Task CreateAddressAsync_WithOrderData_ReturnsValidAddressId()
    {
        // Arrange
        LogTestStart(nameof(CreateAddressAsync_WithOrderData_ReturnsValidAddressId));
        var customerId = TestDataProvider.GetTestCustomerId();
        var uniqueTestId = GenerateTestId();

        // Create unique address name to avoid conflicts
        var testAddressName = $"{_testOrderData.Name}_TEST_{uniqueTestId}";

        try
        {
            // Create address request object
            var addressRequest = new CermApiConnector.Models.CreateAddressRequest
            {
                CustomerId = customerId,
                Name = $"{_testOrderData.Name}_TEST_{uniqueTestId}",
                Street = _testOrderData.Street,
                PostalCode = _testOrderData.PostalCode,
                City = _testOrderData.City,
                Country = _testOrderData.Country,
                IsDeliveryAddress = true,
                IsInvoiceAddress = false
            };

            Logger.LogInformation("Creating address with request: {AddressRequest}", SerializeObject(addressRequest));

            // Act
            var addressResponse = await WithTimeoutAsync(
                CermApiClient.CreateAddressAsync(addressRequest),
                TimeSpan.FromSeconds(30),
                "CreateAddressAsync"
            );

            // Assert
            addressResponse.Should().NotBeNull("Address creation should return a response");
            addressResponse.Success.Should().BeTrue("Address creation should succeed");
            addressResponse.AddressId.Should().NotBeNullOrEmpty("Address ID should not be empty");

            Logger.LogInformation("Address created successfully: AddressId={AddressId}", addressResponse.AddressId);

            LogTestComplete(nameof(CreateAddressAsync_WithOrderData_ReturnsValidAddressId), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Address creation test failed: {Message}", ex.Message);
            LogTestComplete(nameof(CreateAddressAsync_WithOrderData_ReturnsValidAddressId), false);
            throw;
        }
    }

    [Fact]
    public async Task ValidateAddressIdAsync_WithValidId_ReturnsAddressDetails()
    {
        // Arrange
        LogTestStart(nameof(ValidateAddressIdAsync_WithValidId_ReturnsAddressDetails));

        try
        {
            // First, try to get an existing address ID or create one
            var customerId = TestDataProvider.GetTestCustomerId();
            var addressResponse = await CermApiClient.FetchAddressIdAsync(
                customerId,
                _testOrderData.PostalCode,
                _testOrderData.Street,
                _testOrderData.City,
                _testOrderData.Country
            );

            string addressId;

            // If no existing address, create one for testing
            if (!addressResponse.Success || string.IsNullOrEmpty(addressResponse.AddressId))
            {
                Logger.LogInformation("No existing address found, creating one for validation test");

                var addressRequest = new CermApiConnector.Models.CreateAddressRequest
                {
                    CustomerId = customerId,
                    Name = $"{_testOrderData.Name}_VALIDATION_TEST_{GenerateTestId()}",
                    Street = _testOrderData.Street,
                    PostalCode = _testOrderData.PostalCode,
                    City = _testOrderData.City,
                    Country = _testOrderData.Country,
                    IsDeliveryAddress = true,
                    IsInvoiceAddress = false
                };

                var createResponse = await CermApiClient.CreateAddressAsync(addressRequest);
                addressId = createResponse.AddressId;
            }
            else
            {
                addressId = addressResponse.AddressId;
            }

            addressId.Should().NotBeNullOrEmpty("Address ID should be available for validation test");

            // Act
            var addressDetails = await WithTimeoutAsync(
                CermApiClient.ValidateAddressIdAsync(addressId),
                TimeSpan.FromSeconds(30),
                "ValidateAddressIdAsync"
            );

            // Assert
            addressDetails.Should().NotBeNull("Address validation should return address details");

            Logger.LogInformation("Address validation successful: {AddressDetails}",
                SerializeObject(addressDetails));

            LogTestComplete(nameof(ValidateAddressIdAsync_WithValidId_ReturnsAddressDetails), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Address validation test failed: {Message}", ex.Message);
            LogTestComplete(nameof(ValidateAddressIdAsync_WithValidId_ReturnsAddressDetails), false);
            throw;
        }
    }

    [Fact]
    public async Task CreateAndFetchAddress_EndToEndWorkflow_WorksCorrectly()
    {
        // Arrange
        LogTestStart(nameof(CreateAndFetchAddress_EndToEndWorkflow_WorksCorrectly));
        var customerId = TestDataProvider.GetTestCustomerId();
        var uniqueTestId = GenerateTestId();

        try
        {
            // Step 1: Create a new address
            var addressRequest = new CermApiConnector.Models.CreateAddressRequest
            {
                CustomerId = customerId,
                Name = $"{_testOrderData.Name}_E2E_TEST_{uniqueTestId}",
                Street = _testOrderData.Street,
                PostalCode = _testOrderData.PostalCode,
                City = _testOrderData.City,
                Country = _testOrderData.Country,
                IsDeliveryAddress = true,
                IsInvoiceAddress = false
            };

            Logger.LogInformation("Step 1: Creating address with request: {AddressRequest}", SerializeObject(addressRequest));

            var createdAddressResponse = await WithTimeoutAsync(
                CermApiClient.CreateAddressAsync(addressRequest),
                TimeSpan.FromSeconds(30),
                "CreateAddressAsync (Step 1)"
            );

            createdAddressResponse.Should().NotBeNull("Address creation should succeed");
            createdAddressResponse.Success.Should().BeTrue("Address creation should be successful");
            Logger.LogInformation("Step 1 Complete: Address created with ID: {AddressId}", createdAddressResponse.AddressId);

            // Step 2: Validate the created address
            Logger.LogInformation("Step 2: Validating created address");
            var addressDetails = await WithTimeoutAsync(
                CermApiClient.ValidateAddressIdAsync(createdAddressResponse.AddressId),
                TimeSpan.FromSeconds(30),
                "ValidateAddressIdAsync (Step 2)"
            );

            addressDetails.Should().NotBeNull("Address validation should succeed");
            Logger.LogInformation("Step 2 Complete: Address validated: {AddressDetails}",
                SerializeObject(addressDetails));

            // Step 3: Try to fetch the address using search criteria
            Logger.LogInformation("Step 3: Fetching address using search criteria");
            var fetchedAddressResponse = await WithTimeoutAsync(
                CermApiClient.FetchAddressIdAsync(
                    customerId,
                    _testOrderData.PostalCode,
                    _testOrderData.Street,
                    _testOrderData.City,
                    _testOrderData.Country
                ),
                TimeSpan.FromSeconds(30),
                "FetchAddressIdAsync (Step 3)"
            );

            // Note: The fetched address might be different from created one if multiple addresses exist
            // This is expected behavior
            Logger.LogInformation("Step 3 Complete: Fetched address response: Success={Success}, AddressId={AddressId}",
                fetchedAddressResponse.Success, fetchedAddressResponse.AddressId);

            LogTestComplete(nameof(CreateAndFetchAddress_EndToEndWorkflow_WorksCorrectly), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Address end-to-end workflow test failed: {Message}", ex.Message);
            LogTestComplete(nameof(CreateAndFetchAddress_EndToEndWorkflow_WorksCorrectly), false);
            throw;
        }
    }

    [Fact]
    public async Task AddressOperations_PerformanceTest_CompletesWithinTimeout()
    {
        // Arrange
        LogTestStart(nameof(AddressOperations_PerformanceTest_CompletesWithinTimeout));
        var customerId = TestDataProvider.GetTestCustomerId();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Test address fetch performance
            var fetchStartTime = stopwatch.ElapsedMilliseconds;
            var addressResponse = await WithTimeoutAsync(
                CermApiClient.FetchAddressIdAsync(
                    customerId,
                    _testOrderData.PostalCode,
                    _testOrderData.Street,
                    _testOrderData.City,
                    _testOrderData.Country
                ),
                TimeSpan.FromSeconds(10), // Stricter timeout for performance test
                "FetchAddressIdAsync (performance test)"
            );
            var fetchTime = stopwatch.ElapsedMilliseconds - fetchStartTime;

            Logger.LogInformation("Address fetch performance: {FetchTime}ms", fetchTime);
            fetchTime.Should().BeLessThan(10000, "Address fetch should complete within 10 seconds");

            // If we have an address, test validation performance
            if (addressResponse.Success && !string.IsNullOrEmpty(addressResponse.AddressId))
            {
                var validateStartTime = stopwatch.ElapsedMilliseconds;
                var addressDetails = await WithTimeoutAsync(
                    CermApiClient.ValidateAddressIdAsync(addressResponse.AddressId),
                    TimeSpan.FromSeconds(10),
                    "ValidateAddressIdAsync (performance test)"
                );
                var validateTime = stopwatch.ElapsedMilliseconds - validateStartTime;

                Logger.LogInformation("Address validation performance: {ValidateTime}ms", validateTime);
                validateTime.Should().BeLessThan(10000, "Address validation should complete within 10 seconds");
            }

            stopwatch.Stop();
            Logger.LogInformation("Total address operations performance: {TotalTime}ms",
                stopwatch.ElapsedMilliseconds);

            LogTestComplete(nameof(AddressOperations_PerformanceTest_CompletesWithinTimeout), true);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(ex, "Address performance test failed after {ElapsedMs}ms: {Message}",
                stopwatch.ElapsedMilliseconds, ex.Message);
            LogTestComplete(nameof(AddressOperations_PerformanceTest_CompletesWithinTimeout), false);
            throw;
        }
    }
}
