using CermApiConnector.Configuration;
using CermApiConnector.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CermApiConnector.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the CERM API client to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddCermApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        // Register the CermApiSettings
        services.Configure<CermApiSettings>(configuration.GetSection("CermApiSettings"));
        
        // Register the HttpClient for the CermApiClient
        services.AddHttpClient<CermApiClient>();
        
        // Register the CermApiClient
        services.AddTransient<CermApiClient>();
        
        return services;
    }
}
