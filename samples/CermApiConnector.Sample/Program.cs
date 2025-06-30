using CermApiConnector.Extensions;
using CermApiConnector.Models;
using CermApiConnector.Services;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CermApiConnector.Sample;

class Program
{
    static async Task Main(string[] args)
    {
        // Load .env file if it exists
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
        }

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables()
            .Build();

        // Setup dependency injection
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add CERM API client
        services.AddCermApiClient(configuration);

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Get logger and CERM API client
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var cermApiClient = serviceProvider.GetRequiredService<CermApiClient>();

        logger.LogInformation("=== CERM API Connector Sample Application ===");

        try
        {
            // Example 1: Test Authentication
            await TestAuthenticationAsync(cermApiClient, logger);

            // Example 2: Address Management
            await TestAddressManagementAsync(cermApiClient, logger);

            // Example 3: Complete Workflow
            await TestCompleteWorkflowAsync(cermApiClient, logger);

            logger.LogInformation("=== Sample application completed successfully! ===");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sample application failed: {Message}", ex.Message);
        }
    }

    static async Task TestAuthenticationAsync(CermApiClient cermApiClient, ILogger logger)
    {
        logger.LogInformation("\n--- Testing Authentication ---");
        
        try
        {
            var token = await cermApiClient.GetTokenAsync();
            
            if (!string.IsNullOrEmpty(token.AccessToken))
            {
                logger.LogInformation("✅ Authentication successful!");
                logger.LogInformation("Token Type: {TokenType}", token.TokenType);
                logger.LogInformation("Expires In: {ExpiresIn} seconds", token.ExpiresIn);
            }
            else
            {
                logger.LogWarning("❌ Authentication failed - no token received");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Authentication failed: {Message}", ex.Message);
        }
    }

    static async Task TestAddressManagementAsync(CermApiClient cermApiClient, ILogger logger)
    {
        logger.LogInformation("\n--- Testing Address Management ---");

        try
        {
            // Test fetching an existing address
            logger.LogInformation("Searching for existing address...");
            var addressIdResponse = await cermApiClient.FetchAddressIdAsync(
                customerId: "100001",
                postalCode: "4814TT",
                street: "Main Street",
                city: "Breda",
                countryId: "NL"
            );

            if (addressIdResponse.Success && !string.IsNullOrEmpty(addressIdResponse.AddressId))
            {
                logger.LogInformation("✅ Found existing address: {AddressId}", addressIdResponse.AddressId);
                
                // Validate the address
                var validation = await cermApiClient.ValidateAddressIdAsync(addressIdResponse.AddressId);
                if (validation.Success && validation.Exists)
                {
                    logger.LogInformation("✅ Address validation successful");
                    logger.LogInformation("Address Name: {Name}", validation.Name);
                    logger.LogInformation("Address Street: {Street}", validation.Street);
                    logger.LogInformation("Address City: {City}", validation.City);
                }
            }
            else
            {
                logger.LogInformation("No existing address found, creating a new one...");
                
                // Create a new address
                var createRequest = new CreateAddressRequest
                {
                    CustomerId = "100001",
                    Name = $"Sample Address {DateTime.Now:yyyyMMddHHmmss}",
                    Street = "Sample Street 123",
                    PostalCode = "1234AB",
                    City = "Sample City",
                    CountryId = "NL",
                    Email = "sample@example.com",
                    Phone = "+31123456789",
                    IsDeliveryAddress = true,
                    IsInvoiceAddress = false
                };

                var createResponse = await cermApiClient.CreateAddressAsync(createRequest);
                
                if (createResponse.Success)
                {
                    logger.LogInformation("✅ Address created successfully: {AddressId}", createResponse.AddressId);
                }
                else
                {
                    logger.LogWarning("❌ Address creation failed: {Error}", createResponse.Error);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Address management test failed: {Message}", ex.Message);
        }
    }

    static async Task TestCompleteWorkflowAsync(CermApiClient cermApiClient, ILogger logger)
    {
        logger.LogInformation("\n--- Testing Complete Workflow ---");

        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            
            // Step 1: Create a calculation
            logger.LogInformation("Step 1: Creating calculation...");
            var calculationData = new
            {
                Description = $"Sample Calculation {timestamp}",
                Reference = $"REF_{timestamp}",
                Quantity = 1000,
                DeliveryDate = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"),
                CustomerId = "100001"
            };

            var calculationJson = System.Text.Json.JsonSerializer.Serialize(calculationData, 
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            var calculationResponse = await cermApiClient.CreateCalculationWithJsonAsync(calculationJson);
            
            if (calculationResponse.Success)
            {
                logger.LogInformation("✅ Calculation created: {CalculationId}", calculationResponse.CalculationId);

                // Step 2: Create a product
                logger.LogInformation("Step 2: Creating product...");
                var productResponse = await cermApiClient.CreateProductAsync(
                    calculationResponse.CalculationId!,
                    $"Sample Product {timestamp}",
                    1000,
                    25.50m
                );

                if (productResponse.Success)
                {
                    logger.LogInformation("✅ Product created: {ProductId}", productResponse.ProductId);
                    logger.LogInformation("✅ Complete workflow successful!");
                }
                else
                {
                    logger.LogWarning("❌ Product creation failed: {Error}", productResponse.Error);
                }
            }
            else
            {
                logger.LogWarning("❌ Calculation creation failed: {Error}", calculationResponse.Error);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Complete workflow test failed: {Message}", ex.Message);
        }
    }
}
