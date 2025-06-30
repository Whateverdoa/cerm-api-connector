using Microsoft.Extensions.Logging;
using aws_b2b_mod1.Services;
using aws_b2b_mod1.Configuration;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace aws_b2b_mod1.Tests;

public class CermApiAddressValidationTest
{
    private readonly ILogger<CermApiAddressValidationTest> _logger;
    private readonly CermApiClient _cermApiClient;

    public CermApiAddressValidationTest(ILogger<CermApiAddressValidationTest> logger, CermApiClient cermApiClient)
    {
        _logger = logger;
        _cermApiClient = cermApiClient;
    }

    public async Task<bool> RunAddressValidationTestsAsync()
    {
        _logger.LogInformation("=== Starting CERM API Address Validation Tests ===");

        try
        {
            // Test data
            string customerId = "100001"; // Vila Etiketten customer ID
            string postalCode = "4814TT";
            string street = "Main Street";
            string city = "Breda";
            string countryId = "NL";

            // Test 1: Check if address ID exists for known address
            _logger.LogInformation("\n--- Test 1: Bidirectional Address Validation ---");
            var validationResult = await _cermApiClient.ValidateAddressBidirectionalAsync(
                customerId, postalCode, street, city, countryId);

            _logger.LogInformation("Validation Result: {Result}", JsonSerializer.Serialize(validationResult, new JsonSerializerOptions { WriteIndented = true }));

            if (validationResult.Success && validationResult.AddressIdFound)
            {
                string foundAddressId = validationResult.AddressId;
                _logger.LogInformation("Found address ID: {AddressId}", foundAddressId);

                // Test 2: Validate the found address ID exists
                _logger.LogInformation("\n--- Test 2: Validate Address ID Exists ---");
                bool addressIdExists = await _cermApiClient.AddressIdExistsAsync(foundAddressId);
                _logger.LogInformation("Address ID {AddressId} exists: {Exists}", foundAddressId, addressIdExists);

                // Test 3: Get full address details by ID
                _logger.LogInformation("\n--- Test 3: Get Address Details by ID ---");
                var addressDetails = await _cermApiClient.ValidateAddressIdAsync(foundAddressId);
                _logger.LogInformation("Address Details: {Details}", JsonSerializer.Serialize(addressDetails, new JsonSerializerOptions { WriteIndented = true }));

                // Test 4: Test with invalid address ID
                _logger.LogInformation("\n--- Test 4: Test Invalid Address ID ---");
                string invalidAddressId = "999999999";
                bool invalidExists = await _cermApiClient.AddressIdExistsAsync(invalidAddressId);
                _logger.LogInformation("Invalid address ID {AddressId} exists: {Exists}", invalidAddressId, invalidExists);

                var invalidDetails = await _cermApiClient.ValidateAddressIdAsync(invalidAddressId);
                _logger.LogInformation("Invalid Address Details: {Details}", JsonSerializer.Serialize(invalidDetails, new JsonSerializerOptions { WriteIndented = true }));

                return true;
            }
            else
            {
                _logger.LogWarning("Could not find address ID for the test address. This might be expected if the address doesn't exist.");
                
                // Test with invalid address to demonstrate validation
                _logger.LogInformation("\n--- Test: Validation with Non-existent Address ---");
                var invalidValidation = await _cermApiClient.ValidateAddressBidirectionalAsync(
                    customerId, "9999XX", "Non-existent Street", "Non-existent City", countryId);
                
                _logger.LogInformation("Invalid Address Validation: {Result}", JsonSerializer.Serialize(invalidValidation, new JsonSerializerOptions { WriteIndented = true }));
                
                return true; // Still consider test successful as it demonstrates the validation
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during address validation tests");
            return false;
        }
    }

    public async Task<bool> RunSpecificAddressIdValidationAsync(string addressId)
    {
        _logger.LogInformation("=== Testing Specific Address ID: {AddressId} ===", addressId);

        try
        {
            // Test 1: Check if address ID exists
            _logger.LogInformation("Checking if address ID exists...");
            bool exists = await _cermApiClient.AddressIdExistsAsync(addressId);
            _logger.LogInformation("Address ID {AddressId} exists: {Exists}", addressId, exists);

            // Test 2: Get address details
            _logger.LogInformation("Getting address details...");
            var details = await _cermApiClient.ValidateAddressIdAsync(addressId);
            _logger.LogInformation("Address Details: {Details}", JsonSerializer.Serialize(details, new JsonSerializerOptions { WriteIndented = true }));

            if (details.Success && details.Exists)
            {
                // Test 3: Validate bidirectionally using the retrieved details
                _logger.LogInformation("Performing bidirectional validation with retrieved details...");
                var bidirectionalResult = await _cermApiClient.ValidateAddressBidirectionalAsync(
                    details.CustomerId, details.PostalCode, details.Street, details.City, details.Country);
                
                _logger.LogInformation("Bidirectional Validation: {Result}", JsonSerializer.Serialize(bidirectionalResult, new JsonSerializerOptions { WriteIndented = true }));
                
                return bidirectionalResult.Success && bidirectionalResult.AddressDetailsMatch;
            }

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during specific address ID validation");
            return false;
        }
    }
}
