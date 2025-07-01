using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CermApiConnector.Configuration;
using CermApiConnector.Services;
using CermApiConnector.Extensions;
using System.Text.Json;
using DotNetEnv;

namespace CermApiModule.Tests;

/// <summary>
/// Base class for all CERM API tests providing common setup and utilities
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly CermApiClient CermApiClient;
    protected readonly ILogger Logger;
    protected readonly IConfiguration Configuration;

    protected TestBase()
    {
        // Load .env file if it exists
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
        }

        // Build configuration
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddUserSecrets<TestBase>()
            .AddEnvironmentVariables()
            .Build();

        // Setup services
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddConfiguration(Configuration.GetSection("Logging"));
        });

        // Add CERM API client
        services.AddCermApiClient(Configuration);

        // Build service provider
        ServiceProvider = services.BuildServiceProvider();

        // Get services
        CermApiClient = ServiceProvider.GetRequiredService<CermApiClient>();
        Logger = ServiceProvider.GetRequiredService<ILogger<TestBase>>();

        Logger.LogInformation("Test base initialized for {TestClass}", GetType().Name);
    }

    /// <summary>
    /// Helper method to log test start
    /// </summary>
    protected void LogTestStart(string testName)
    {
        Logger.LogInformation("=== Starting test: {TestName} ===", testName);
    }

    /// <summary>
    /// Helper method to log test completion
    /// </summary>
    protected void LogTestComplete(string testName, bool success)
    {
        var status = success ? "PASSED" : "FAILED";
        Logger.LogInformation("=== Test {TestName} {Status} ===", testName, status);
    }

    /// <summary>
    /// Helper method to serialize objects for logging
    /// </summary>
    protected string SerializeObject(object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    /// <summary>
    /// Helper method to generate unique test identifiers
    /// </summary>
    protected string GenerateTestId()
    {
        return $"TEST_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
    }

    /// <summary>
    /// Helper method to wait for async operations with timeout
    /// </summary>
    protected async Task<T> WithTimeoutAsync<T>(Task<T> task, TimeSpan timeout, string operationName)
    {
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            return await task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Logger.LogError("Operation {OperationName} timed out after {Timeout}", operationName, timeout);
            throw new TimeoutException($"Operation {operationName} timed out after {timeout}");
        }
    }

    public virtual void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
