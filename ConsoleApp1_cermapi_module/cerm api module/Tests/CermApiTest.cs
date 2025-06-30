using aws_b2b_mod1.Models;
using aws_b2b_mod1.Services;
using Microsoft.Extensions.Logging;

namespace aws_b2b_mod1.Tests;

public class CermApiTest
{
    private readonly CermApiClient _cermApiClient;
    private readonly ILogger<CermApiTest> _logger;

    public CermApiTest(CermApiClient cermApiClient, ILogger<CermApiTest> logger)
    {
        _cermApiClient = cermApiClient;
        _logger = logger;
    }

    public async Task<bool> RunTestAsync()
    {
        _logger.LogInformation("Starting CERM API test...");
        
        try
        {
            // Test getting a token
            _logger.LogInformation("Testing token retrieval...");
            var token = await _cermApiClient.GetTokenAsync();
            
            if (string.IsNullOrEmpty(token.AccessToken))
            {
                _logger.LogError("Failed to get token");
                return false;
            }
            
            _logger.LogInformation("Successfully got token: {TokenType} {AccessToken}", token.TokenType, token.AccessToken.Substring(0, 10) + "...");
            
            // Test fetching an address ID
            _logger.LogInformation("Testing address ID retrieval...");
            var addressIdResponse = await _cermApiClient.FetchAddressIdAsync(
                "CUSTOMER123", // Replace with a valid customer ID
                "1234 AB", // Replace with a valid postal code
                "123 Main Street", // Replace with a valid street
                "Amsterdam", // Replace with a valid city
                "NL" // Replace with a valid country ID
            );
            
            if (!addressIdResponse.Success)
            {
                _logger.LogWarning("Failed to fetch address ID: {Error}", addressIdResponse.Error);
                _logger.LogInformation("This might be expected if the address doesn't exist");
                
                // Test creating an address
                _logger.LogInformation("Testing address creation...");
                var createAddressRequest = new CreateAddressRequest
                {
                    CustomerId = "CUSTOMER123", // Replace with a valid customer ID
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
                
                var createAddressResponse = await _cermApiClient.CreateAddressAsync(createAddressRequest);
                
                if (!createAddressResponse.Success)
                {
                    _logger.LogError("Failed to create address: {Error}", createAddressResponse.Error);
                    return false;
                }
                
                _logger.LogInformation("Successfully created address: {AddressId}", createAddressResponse.AddressId);
            }
            else
            {
                _logger.LogInformation("Successfully fetched address ID: {AddressId}", addressIdResponse.AddressId);
            }
            
            _logger.LogInformation("CERM API test completed successfully!");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CERM API test: {Message}", ex.Message);
            return false;
        }
    }
}
