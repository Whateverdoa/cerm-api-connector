using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace CermApiModule.Tests;

/// <summary>
/// Tests for CERM API authentication functionality
/// </summary>
public class AuthenticationTests : TestBase
{
    public AuthenticationTests() : base()
    {
        Logger.LogInformation("AuthenticationTests initialized");
    }

    [Fact]
    public async Task GetTokenAsync_ValidCredentials_ReturnsValidToken()
    {
        // Arrange
        LogTestStart(nameof(GetTokenAsync_ValidCredentials_ReturnsValidToken));

        try
        {
            // Act
            var token = await WithTimeoutAsync(
                CermApiClient.GetTokenAsync(),
                TimeSpan.FromSeconds(30),
                "GetTokenAsync"
            );

            // Assert
            token.Should().NotBeNull("Token response should not be null");
            token.AccessToken.Should().NotBeNullOrEmpty("Access token should not be null or empty");
            token.TokenType.Should().NotBeNullOrEmpty("Token type should not be null or empty");
            token.ExpiresIn.Should().BeGreaterThan(0, "Token expiration should be greater than 0");

            Logger.LogInformation("Token retrieved successfully: Type={TokenType}, ExpiresIn={ExpiresIn}s",
                token.TokenType, token.ExpiresIn);

            LogTestComplete(nameof(GetTokenAsync_ValidCredentials_ReturnsValidToken), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Authentication test failed: {Message}", ex.Message);
            LogTestComplete(nameof(GetTokenAsync_ValidCredentials_ReturnsValidToken), false);
            throw;
        }
    }

    [Fact]
    public async Task GetTokenAsync_CachedToken_ReusesToken()
    {
        // Arrange
        LogTestStart(nameof(GetTokenAsync_CachedToken_ReusesToken));

        try
        {
            // Act - Get token twice
            var token1 = await WithTimeoutAsync(
                CermApiClient.GetTokenAsync(),
                TimeSpan.FromSeconds(30),
                "GetTokenAsync (first call)"
            );

            var token2 = await WithTimeoutAsync(
                CermApiClient.GetTokenAsync(),
                TimeSpan.FromSeconds(30),
                "GetTokenAsync (second call)"
            );

            // Assert
            token1.Should().NotBeNull("First token should not be null");
            token2.Should().NotBeNull("Second token should not be null");
            token1.AccessToken.Should().Be(token2.AccessToken, "Cached token should be the same");

            Logger.LogInformation("Token caching verified - same token returned on subsequent calls");

            LogTestComplete(nameof(GetTokenAsync_CachedToken_ReusesToken), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Token caching test failed: {Message}", ex.Message);
            LogTestComplete(nameof(GetTokenAsync_CachedToken_ReusesToken), false);
            throw;
        }
    }

    [Fact]
    public async Task GetTokenAsync_TokenExpiration_ChecksExpiryCorrectly()
    {
        // Arrange
        LogTestStart(nameof(GetTokenAsync_TokenExpiration_ChecksExpiryCorrectly));

        try
        {
            // Act
            var token = await WithTimeoutAsync(
                CermApiClient.GetTokenAsync(),
                TimeSpan.FromSeconds(30),
                "GetTokenAsync"
            );

            // Assert
            token.Should().NotBeNull("Token should not be null");
            token.ExpiresAt.Should().BeAfter(DateTime.UtcNow, "Token expiry should be in the future");
            token.IsExpired.Should().BeFalse("Token should not be expired immediately after retrieval");

            var expectedExpiryTime = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
            token.ExpiresAt.Should().BeCloseTo(expectedExpiryTime, TimeSpan.FromSeconds(5),
                "Token expiry time should be calculated correctly");

            Logger.LogInformation("Token expiry validation passed - ExpiresAt={ExpiresAt}, IsExpired={IsExpired}",
                token.ExpiresAt, token.IsExpired);

            LogTestComplete(nameof(GetTokenAsync_TokenExpiration_ChecksExpiryCorrectly), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Token expiration test failed: {Message}", ex.Message);
            LogTestComplete(nameof(GetTokenAsync_TokenExpiration_ChecksExpiryCorrectly), false);
            throw;
        }
    }

    [Fact]
    public async Task GetTokenAsync_MultipleRequests_HandlesCorrectly()
    {
        // Arrange
        LogTestStart(nameof(GetTokenAsync_MultipleRequests_HandlesCorrectly));

        try
        {
            // Act - Make multiple concurrent requests
            var tasks = new List<Task<CermApiConnector.Models.TokenResponse>>();
            for (int i = 0; i < 5; i++)
            {
                tasks.Add(CermApiClient.GetTokenAsync());
            }

            var tokens = await Task.WhenAll(tasks);

            // Assert
            tokens.Should().HaveCount(5, "All token requests should complete");
            tokens.Should().OnlyContain(t => t != null, "All tokens should be non-null");
            tokens.Should().OnlyContain(t => !string.IsNullOrEmpty(t.AccessToken),
                "All tokens should have access tokens");

            // All tokens should be the same due to caching
            var firstToken = tokens[0].AccessToken;
            tokens.Should().OnlyContain(t => t.AccessToken == firstToken,
                "All concurrent requests should return the same cached token");

            Logger.LogInformation("Multiple concurrent token requests handled correctly");

            LogTestComplete(nameof(GetTokenAsync_MultipleRequests_HandlesCorrectly), true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Multiple requests test failed: {Message}", ex.Message);
            LogTestComplete(nameof(GetTokenAsync_MultipleRequests_HandlesCorrectly), false);
            throw;
        }
    }

    [Fact]
    public async Task GetTokenAsync_PerformanceTest_CompletesWithinTimeout()
    {
        // Arrange
        LogTestStart(nameof(GetTokenAsync_PerformanceTest_CompletesWithinTimeout));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // Act
            var token = await WithTimeoutAsync(
                CermApiClient.GetTokenAsync(),
                TimeSpan.FromSeconds(10), // Stricter timeout for performance test
                "GetTokenAsync (performance test)"
            );

            stopwatch.Stop();

            // Assert
            token.Should().NotBeNull("Token should be retrieved within timeout");
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000,
                "Token retrieval should complete within 10 seconds");

            Logger.LogInformation("Token retrieval performance: {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);

            LogTestComplete(nameof(GetTokenAsync_PerformanceTest_CompletesWithinTimeout), true);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Logger.LogError(ex, "Performance test failed after {ElapsedMs}ms: {Message}",
                stopwatch.ElapsedMilliseconds, ex.Message);
            LogTestComplete(nameof(GetTokenAsync_PerformanceTest_CompletesWithinTimeout), false);
            throw;
        }
    }
}
