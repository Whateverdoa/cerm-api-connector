using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using aws_b2b_mod1.Configuration;
using aws_b2b_mod1.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace aws_b2b_mod1.Services;

public class CermApiClient
{
    private readonly HttpClient _httpClient;
    private readonly CermApiSettings _settings;
    private readonly ILogger<CermApiClient> _logger;
    private Models.TokenResponse? _cachedToken;
    private DateTime _tokenExpiryTime = DateTime.MinValue;

    public CermApiClient(HttpClient httpClient, IOptions<CermApiSettings> settings, ILogger<CermApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        // Don't set BaseAddress or default headers here
        // We'll set them for each request

        // Load the environment from user secrets
        var environment = _settings.Environment;
        _logger.LogInformation("CERM API client initialized for environment: {Environment}", environment);
        _logger.LogInformation("Base URL: {BaseUrl}", _settings.GetBaseUrl());
        _logger.LogInformation("Host Header: {HostHeader}", _settings.GetHostHeader());
    }

    /// <summary>
    /// Gets a token from the CERM API
    /// </summary>
    /// <returns>A TokenResponse containing the token</returns>
    public async Task<Models.TokenResponse> GetTokenAsync()
    {
        // Check if we have a cached token that's still valid
        if (_cachedToken != null && DateTime.UtcNow < _tokenExpiryTime)
        {
            _logger.LogDebug("Using cached token");
            return _cachedToken;
        }

        _logger.LogInformation("Getting new token from CERM API");

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Build the token URL
        string url = baseUrl + _settings.Paths.Token;
        _logger.LogInformation("Token URL: {Url}", url);

        // Get the credentials
        string clientId = _settings.ClientId;
        string clientSecret = _settings.ClientSecret;
        string username = _settings.Username;
        string password = _settings.Password;

        _logger.LogInformation("Using credentials - ClientId: '{ClientId}', Username: '{Username}'",
            clientId,
            username);

        // Prepare the request
        var request = new HttpRequestMessage(HttpMethod.Post, url);

        // Add headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        request.Headers.Add("Host", hostHeader);

        // Add Basic Authentication
        var authBytes = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
        var authHeader = Convert.ToBase64String(authBytes);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);
        _logger.LogInformation("Added HTTP Basic Auth header");

        // Add form content
        var formContent = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password)
        });

        // Log the request details
        _logger.LogInformation("Token request headers: {Headers}", request.Headers);
        _logger.LogInformation("Token request content: {Content}", await formContent.ReadAsStringAsync());

        // Set the content
        request.Content = formContent;

        // Send the request
        var response = await _httpClient.SendAsync(request);

        // Check if the request was successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to get token: {StatusCode} - {Content}", response.StatusCode, errorContent);
            return new Models.TokenResponse();
        }

        // Parse the response
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Token response: {Response}", responseContent);

        try
        {
            // Configure JsonSerializer options to handle property name mapping
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var tokenResponse = JsonSerializer.Deserialize<Models.TokenResponse>(responseContent, options);

            if (tokenResponse == null)
            {
                _logger.LogError("Failed to deserialize token response");
                return new Models.TokenResponse();
            }

            // Cache the token
            _cachedToken = tokenResponse;
            _tokenExpiryTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Expire 1 minute early to be safe

            _logger.LogInformation("Successfully got token from CERM API");
            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing token response: {Message}", ex.Message);
            return new Models.TokenResponse();
        }
    }

    /// <summary>
    /// Fetches an address ID from the CERM API
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="postalCode">The postal code</param>
    /// <param name="street">The street</param>
    /// <param name="city">The city</param>
    /// <param name="countryId">The country ID</param>
    /// <returns>An AddressIdResponse containing the address ID</returns>
    public async Task<AddressIdResponse> FetchAddressIdAsync(string customerId, string postalCode, string street, string city, string countryId)
    {
        _logger.LogInformation("Fetching address ID from CERM API for customer {CustomerId}", customerId);

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.FetchAddressId;

        // Create query parameters - matching the Python implementation
        var queryParams = new Dictionary<string, string>
        {
            { "customerid", customerId },
            { "postalcode", postalCode },
            { "street", street.Length > 40 ? street.Substring(0, 40) : street },
            { "city", city },
            { "countryid", countryId }
        };

        // Build query string
        var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
        var requestUrl = $"{endpoint}?{queryString}";

        _logger.LogInformation("Fetch address ID URL: {Url}", requestUrl);

        // Create an explicit HttpRequestMessage for better control
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        // Add headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        request.Headers.Add("Host", hostHeader);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Send the request
        var response = await _httpClient.SendAsync(request);

        // Check if the request was successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to fetch address ID: {StatusCode} - {Content}", response.StatusCode, errorContent);

            // Return a failed response
            return new AddressIdResponse
            {
                Success = false,
                Error = $"Failed to fetch address ID: {response.StatusCode} - {errorContent}"
            };
        }

        // Parse the response
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Address ID response: {Response}", responseContent);

        try
        {
            // The response format is an array of address objects
            using JsonDocument doc = JsonDocument.Parse(responseContent);
            JsonElement root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
            {
                var firstAddress = root[0];
                if (firstAddress.TryGetProperty("AddressID", out JsonElement addressIdElement))
                {
                    string addressId = addressIdElement.GetString() ?? string.Empty;

                    var addressIdResponse = new AddressIdResponse
                    {
                        AddressId = addressId,
                        Success = !string.IsNullOrEmpty(addressId)
                    };

                    _logger.LogInformation("Successfully fetched address ID from CERM API: {AddressId}", addressId);
                    return addressIdResponse;
                }
            }

            // If we get here, no address was found
            _logger.LogWarning("No address found in response");
            return new AddressIdResponse
            {
                Success = false,
                Error = "No address found"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing address ID response: {Message}", ex.Message);
            return new AddressIdResponse
            {
                Success = false,
                Error = $"Error parsing address ID response: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Creates a new address in the CERM API
    /// </summary>
    /// <param name="request">The address creation request</param>
    /// <returns>An AddressIdResponse containing the new address ID</returns>
    public async Task<AddressIdResponse> CreateAddressAsync(CreateAddressRequest request)
    {
        _logger.LogInformation("Creating address in CERM API for customer {CustomerId}", request.CustomerId);

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.CreateAddress;

        // Create an explicit HttpRequestMessage for better control
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
        };

        // Add headers
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        httpRequest.Headers.Add("Host", hostHeader);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Send the request
        var response = await _httpClient.SendAsync(httpRequest);

        // Check if the request was successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create address: {StatusCode} - {Content}", response.StatusCode, errorContent);

            // Return a failed response
            return new AddressIdResponse
            {
                Success = false,
                Error = $"Failed to create address: {response.StatusCode} - {errorContent}"
            };
        }

        // Parse the response
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Create address response: {Response}", responseContent);

        try
        {
            var addressIdResponse = JsonSerializer.Deserialize<AddressIdResponse>(responseContent);

            if (addressIdResponse == null)
            {
                _logger.LogError("Failed to deserialize create address response");
                return new AddressIdResponse
                {
                    Success = false,
                    Error = "Failed to deserialize create address response"
                };
            }

            // Set success flag
            addressIdResponse.Success = !string.IsNullOrEmpty(addressIdResponse.AddressId);

            _logger.LogInformation("Successfully created address in CERM API: {AddressId}", addressIdResponse.AddressId);
            return addressIdResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing create address response: {Message}", ex.Message);
            return new AddressIdResponse
            {
                Success = false,
                Error = $"Error parsing create address response: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Creates a new address in the CERM API using a JSON payload
    /// </summary>
    /// <param name="jsonPayload">The JSON payload for the address creation request</param>
    /// <returns>An AddressIdResponse containing the new address ID</returns>
    public async Task<AddressIdResponse> CreateAddressWithJsonAsync(string jsonPayload)
    {
        _logger.LogInformation("Creating address in CERM API with JSON payload");

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.CreateAddress;

        // Create an explicit HttpRequestMessage for better control
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        // Add headers
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        httpRequest.Headers.Add("Host", hostHeader);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Send the request
        var response = await _httpClient.SendAsync(httpRequest);

        // Check if the request was successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create address: {StatusCode} - {Content}", response.StatusCode, errorContent);

            // Return a failed response
            return new AddressIdResponse
            {
                Success = false,
                Error = $"Failed to create address: {response.StatusCode} - {errorContent}"
            };
        }

        // Parse the response
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Create address response: {Response}", responseContent);

        try
        {
            // The response format is different for the address creation API
            // It returns a JSON object with a Data property that contains the address details
            // We need to extract the Id from the Data property
            using JsonDocument doc = JsonDocument.Parse(responseContent);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("Data", out JsonElement data) &&
                data.TryGetProperty("Id", out JsonElement id))
            {
                string addressId = id.GetString() ?? string.Empty;

                var addressIdResponse = new AddressIdResponse
                {
                    AddressId = addressId,
                    Success = !string.IsNullOrEmpty(addressId)
                };

                _logger.LogInformation("Successfully created address in CERM API: {AddressId}", addressId);
                return addressIdResponse;
            }
            else
            {
                _logger.LogError("Failed to extract address ID from response: {Response}", responseContent);
                return new AddressIdResponse
                {
                    Success = false,
                    Error = $"Failed to extract address ID from response: {responseContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing create address response: {Message}", ex.Message);
            return new AddressIdResponse
            {
                Success = false,
                Error = $"Error parsing create address response: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Fetches a quote ID from the CERM API
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="productCode">The product code</param>
    /// <returns>A QuoteIdResponse containing the quote ID</returns>
    public async Task<QuoteIdResponse> FetchQuoteIdAsync(string customerId, string productCode)
    {
        _logger.LogInformation("Fetching quote ID from CERM API for customer {CustomerId} and product {ProductCode}", customerId, productCode);

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.FetchCalculationId;
        var requestData = new
        {
            customerId,
            productCode
        };

        // Create an explicit HttpRequestMessage for better control
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
        };

        // Add headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        request.Headers.Add("Host", hostHeader);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Send the request
        var response = await _httpClient.SendAsync(request);

        // Check if the request was successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to fetch quote ID: {StatusCode} - {Content}", response.StatusCode, errorContent);

            // Return a failed response
            return new QuoteIdResponse
            {
                Success = false,
                Error = $"Failed to fetch quote ID: {response.StatusCode} - {errorContent}"
            };
        }

        // Parse the response
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Quote ID response: {Response}", responseContent);

        try
        {
            var quoteIdResponse = JsonSerializer.Deserialize<QuoteIdResponse>(responseContent);

            if (quoteIdResponse == null)
            {
                _logger.LogError("Failed to deserialize quote ID response");
                return new QuoteIdResponse
                {
                    Success = false,
                    Error = "Failed to deserialize quote ID response"
                };
            }

            // Set success flag
            quoteIdResponse.Success = !string.IsNullOrEmpty(quoteIdResponse.CalculationId);

            _logger.LogInformation("Successfully fetched quote ID from CERM API: {QuoteId}", quoteIdResponse.CalculationId);
            return quoteIdResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing quote ID response: {Message}", ex.Message);
            return new QuoteIdResponse
            {
                Success = false,
                Error = $"Error parsing quote ID response: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Creates a new product in the CERM API
    /// </summary>
    /// <param name="calculationId">The calculation ID</param>
    /// <param name="productName">The product name</param>
    /// <param name="quantity">The quantity</param>
    /// <param name="unitPrice">The unit price</param>
    /// <returns>A ProductIdResponse containing the new product ID</returns>
    public async Task<ProductIdResponse> CreateProductAsync(string calculationId, string productName, int quantity, decimal unitPrice)
    {
        _logger.LogInformation("Creating product in CERM API for calculation {CalculationId}", calculationId);

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.CreateProduct.Replace("{calculationId}", calculationId);
        var requestData = new
        {
            name = productName,
            quantity,
            unitPrice
        };

        // Create an explicit HttpRequestMessage for better control
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json")
        };

        // Add headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        request.Headers.Add("Host", hostHeader);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Send the request
        var response = await _httpClient.SendAsync(request);

        // Check if the request was successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create product: {StatusCode} - {Content}", response.StatusCode, errorContent);

            // Return a failed response
            return new ProductIdResponse
            {
                Success = false,
                Error = $"Failed to create product: {response.StatusCode} - {errorContent}"
            };
        }

        // Parse the response
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Create product response: {Response}", responseContent);

        try
        {
            var productIdResponse = JsonSerializer.Deserialize<ProductIdResponse>(responseContent);

            if (productIdResponse == null)
            {
                _logger.LogError("Failed to deserialize product creation response");
                return new ProductIdResponse
                {
                    Success = false,
                    Error = "Failed to deserialize product creation response"
                };
            }

            // Set success flag
            productIdResponse.Success = !string.IsNullOrEmpty(productIdResponse.ProductId);

            _logger.LogInformation("Successfully created product in CERM API: {ProductId}", productIdResponse.ProductId);
            return productIdResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing product creation response: {Message}", ex.Message);
            return new ProductIdResponse
            {
                Success = false,
                Error = $"Error parsing product creation response: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Creates a new sales order in the CERM API
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="contactId">The contact ID</param>
    /// <param name="orderData">The order data</param>
    /// <returns>A SalesOrderIdResponse containing the new sales order ID</returns>
    public async Task<SalesOrderIdResponse> CreateSalesOrderAsync(string customerId, string contactId, object orderData)
    {
        _logger.LogInformation("Creating sales order in CERM API for customer {CustomerId} and contact {ContactId}", customerId, contactId);

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.CreateSalesOrder
            .Replace("{customerId}", customerId)
            .Replace("{contactId}", contactId);

        // Create an explicit HttpRequestMessage for better control
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(orderData), Encoding.UTF8, "application/json")
        };

        // Add headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        request.Headers.Add("Host", hostHeader);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Send the request
        var response = await _httpClient.SendAsync(request);

        // Check if the request was successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create sales order: {StatusCode} - {Content}", response.StatusCode, errorContent);

            // Return a failed response
            return new SalesOrderIdResponse
            {
                Success = false,
                Error = $"Failed to create sales order: {response.StatusCode} - {errorContent}"
            };
        }

        // Parse the response
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Create sales order response: {Response}", responseContent);

        try
        {
            var salesOrderIdResponse = JsonSerializer.Deserialize<SalesOrderIdResponse>(responseContent);

            if (salesOrderIdResponse == null)
            {
                _logger.LogError("Failed to deserialize sales order creation response");
                return new SalesOrderIdResponse
                {
                    Success = false,
                    Error = "Failed to deserialize sales order creation response"
                };
            }

            // Set success flag
            salesOrderIdResponse.Success = !string.IsNullOrEmpty(salesOrderIdResponse.SalesOrderId);

            _logger.LogInformation("Successfully created sales order in CERM API: {SalesOrderId}", salesOrderIdResponse.SalesOrderId);
            return salesOrderIdResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing sales order creation response: {Message}", ex.Message);
            return new SalesOrderIdResponse
            {
                Success = false,
                Error = $"Error parsing sales order creation response: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Creates a calculation with a JSON payload
    /// </summary>
    /// <param name="jsonPayload">The JSON payload</param>
    /// <returns>A response containing the calculation ID</returns>
    public async Task<QuoteIdResponse> CreateCalculationWithJsonAsync(string jsonPayload)
    {
        _logger.LogInformation("Creating calculation in CERM API with JSON payload");

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.CreateCalculation;

        // Create an explicit HttpRequestMessage for better control
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        // Add headers
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        httpRequest.Headers.Add("Host", hostHeader);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Send the request
        var response = await _httpClient.SendAsync(httpRequest);

        // Check if the request was successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create calculation: {StatusCode} - {Content}", response.StatusCode, errorContent);

            // Return a failed response
            return new QuoteIdResponse
            {
                Success = false,
                Error = $"Failed to create calculation: {response.StatusCode} - {errorContent}"
            };
        }

        // Parse the response
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Create calculation response: {Response}", responseContent);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(responseContent);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("Data", out JsonElement data) &&
                data.TryGetProperty("Id", out JsonElement id))
            {
                string calculationId = id.GetString() ?? string.Empty;

                var calculationResponse = new QuoteIdResponse
                {
                    CalculationId = calculationId,
                    Success = !string.IsNullOrEmpty(calculationId)
                };

                _logger.LogInformation("Successfully created calculation in CERM API: {CalculationId}", calculationId);
                return calculationResponse;
            }
            else
            {
                _logger.LogError("Failed to extract calculation ID from response: {Response}", responseContent);
                return new QuoteIdResponse
                {
                    Success = false,
                    Error = $"Failed to extract calculation ID from response: {responseContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing create calculation response: {Message}", ex.Message);
            return new QuoteIdResponse
            {
                Success = false,
                Error = $"Error parsing create calculation response: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Creates a new product in the CERM API using a JSON payload
    /// </summary>
    /// <param name="calculationId">The calculation ID</param>
    /// <param name="jsonPayload">The JSON payload for the product creation request</param>
    /// <returns>A ProductIdResponse containing the new product ID</returns>
    public async Task<ProductIdResponse> CreateProductWithJsonAsync(string calculationId, string jsonPayload)
    {
        _logger.LogInformation("Creating product in CERM API for calculation {CalculationId} with JSON payload", calculationId);

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.CreateProduct
            .Replace("{calculationId}", calculationId);

        return await SendProductJsonAsync(endpoint, token, hostHeader, jsonPayload);
    }

    /// <summary>
    /// Creates a product with a JSON payload
    /// </summary>
    /// <param name="jsonPayload">The JSON payload</param>
    /// <returns>A response containing the product ID</returns>
    public async Task<ProductIdResponse> CreateProductWithJsonAsync(string jsonPayload)
    {
        _logger.LogInformation("Creating product in CERM API with JSON payload");

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.CreateProduct;

        return await SendProductJsonAsync(endpoint, token, hostHeader, jsonPayload);
    }

    /// <summary>
    /// Creates a new sales order in the CERM API using a JSON payload
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="contactId">The contact ID</param>
    /// <param name="jsonPayload">The JSON payload for the sales order creation request</param>
    /// <returns>A SalesOrderIdResponse containing the new sales order ID</returns>
    public async Task<SalesOrderIdResponse> CreateSalesOrderWithJsonAsync(string customerId, string contactId, string jsonPayload)
    {
        _logger.LogInformation("Creating sales order in CERM API for customer {CustomerId} and contact {ContactId} with JSON payload", customerId, contactId);

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.CreateSalesOrder
            .Replace("{customerId}", customerId)
            .Replace("{contactId}", contactId);

        return await SendSalesOrderJsonAsync(endpoint, token, hostHeader, jsonPayload);
    }

    /// <summary>
    /// Creates a sales order with a JSON payload
    /// </summary>
    /// <param name="jsonPayload">The JSON payload</param>
    /// <returns>A response containing the sales order ID</returns>
    public async Task<SalesOrderIdResponse> CreateSalesOrderWithJsonAsync(string jsonPayload)
    {
        _logger.LogInformation("Creating sales order in CERM API with JSON payload");

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request
        var endpoint = baseUrl + _settings.Paths.CreateSalesOrder;

        return await SendSalesOrderJsonAsync(endpoint, token, hostHeader, jsonPayload);
    }

    /// <summary>
    /// Validates if an address ID exists in the CERM API by fetching its details
    /// </summary>
    /// <param name="addressId">The address ID to validate</param>
    /// <returns>A response indicating if the address ID exists and contains address details</returns>
    public async Task<AddressDetailsResponse> ValidateAddressIdAsync(string addressId)
    {
        _logger.LogInformation("Validating address ID {AddressId} in CERM API", addressId);

        // Get a token
        var token = await GetTokenAsync();

        // Get the base URL and host header based on the environment
        string baseUrl = _settings.GetBaseUrl();
        string hostHeader = _settings.GetHostHeader();

        // Prepare the request - replace {id} placeholder with actual address ID
        var endpoint = baseUrl + _settings.Paths.FetchAddressById.Replace("{id}", addressId);

        // Create the HTTP request
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
        request.Headers.Add("Host", hostHeader);

        _logger.LogInformation("Sending GET request to: {Endpoint}", endpoint);

        try
        {
            using var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("CERM API response status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("CERM API response content: {Content}", responseContent);

            if (response.IsSuccessStatusCode)
            {
                // Try to parse the response
                using JsonDocument doc = JsonDocument.Parse(responseContent);
                JsonElement root = doc.RootElement;

                // Check if we have address data
                if (root.TryGetProperty("Data", out JsonElement data))
                {
                    var addressDetails = new AddressDetailsResponse
                    {
                        Success = true,
                        AddressId = addressId,
                        Exists = true
                    };

                    // Extract address details if available
                    if (data.TryGetProperty("Id", out JsonElement id))
                        addressDetails.AddressId = id.GetString() ?? addressId;
                    if (data.TryGetProperty("CustomerId", out JsonElement customerId))
                        addressDetails.CustomerId = customerId.GetString() ?? string.Empty;
                    if (data.TryGetProperty("Name", out JsonElement name))
                        addressDetails.Name = name.GetString() ?? string.Empty;
                    if (data.TryGetProperty("Street", out JsonElement street))
                        addressDetails.Street = street.GetString() ?? string.Empty;
                    if (data.TryGetProperty("PostalCode", out JsonElement postalCode))
                        addressDetails.PostalCode = postalCode.GetString() ?? string.Empty;
                    if (data.TryGetProperty("City", out JsonElement city))
                        addressDetails.City = city.GetString() ?? string.Empty;
                    if (data.TryGetProperty("Country", out JsonElement country))
                        addressDetails.Country = country.GetString() ?? string.Empty;
                    if (data.TryGetProperty("Active", out JsonElement active))
                        addressDetails.IsActive = active.GetBoolean();

                    _logger.LogInformation("Address ID {AddressId} exists and is valid", addressId);
                    return addressDetails;
                }
                else
                {
                    // Response doesn't contain expected data structure
                    _logger.LogWarning("Address ID {AddressId} response doesn't contain expected data structure", addressId);
                    return new AddressDetailsResponse
                    {
                        Success = false,
                        AddressId = addressId,
                        Exists = false,
                        Error = "Invalid response format from CERM API"
                    };
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Address ID doesn't exist
                _logger.LogInformation("Address ID {AddressId} does not exist in CERM API", addressId);
                return new AddressDetailsResponse
                {
                    Success = true,
                    AddressId = addressId,
                    Exists = false,
                    Message = "Address ID not found"
                };
            }
            else
            {
                // Other error
                _logger.LogError("Failed to validate address ID {AddressId}. Status: {StatusCode}, Content: {Content}",
                    addressId, response.StatusCode, responseContent);
                return new AddressDetailsResponse
                {
                    Success = false,
                    AddressId = addressId,
                    Exists = false,
                    Error = $"API request failed with status {response.StatusCode}: {responseContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while validating address ID {AddressId}", addressId);
            return new AddressDetailsResponse
            {
                Success = false,
                AddressId = addressId,
                Exists = false,
                Error = $"Exception: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Checks if an address ID exists in the CERM API (lightweight validation)
    /// </summary>
    /// <param name="addressId">The address ID to check</param>
    /// <returns>True if the address ID exists, false otherwise</returns>
    public async Task<bool> AddressIdExistsAsync(string addressId)
    {
        _logger.LogInformation("Checking if address ID {AddressId} exists in CERM API", addressId);

        try
        {
            var result = await ValidateAddressIdAsync(addressId);
            return result.Success && result.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while checking address ID {AddressId} existence", addressId);
            return false;
        }
    }

    /// <summary>
    /// Enhanced address validation that checks both directions:
    /// 1. If the provided address details return a valid address ID
    /// 2. If that address ID can be validated back to address details
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="postalCode">The postal code</param>
    /// <param name="street">The street</param>
    /// <param name="city">The city</param>
    /// <param name="countryId">The country ID</param>
    /// <returns>A comprehensive validation result</returns>
    public async Task<AddressValidationResult> ValidateAddressBidirectionalAsync(
        string customerId, string postalCode, string street, string city, string countryId)
    {
        _logger.LogInformation("Performing bidirectional address validation for customer {CustomerId}", customerId);

        var result = new AddressValidationResult
        {
            CustomerId = customerId,
            PostalCode = postalCode,
            Street = street,
            City = city,
            CountryId = countryId
        };

        try
        {
            // Step 1: Try to fetch address ID from address details
            _logger.LogInformation("Step 1: Fetching address ID from address details");
            var addressIdResponse = await FetchAddressIdAsync(customerId, postalCode, street, city, countryId);

            if (!addressIdResponse.Success || string.IsNullOrEmpty(addressIdResponse.AddressId))
            {
                result.AddressIdFound = false;
                result.Success = false;
                result.Message = "No address ID found for the provided address details";
                result.Error = addressIdResponse.Error;
                return result;
            }

            result.AddressIdFound = true;
            result.AddressId = addressIdResponse.AddressId;
            _logger.LogInformation("Step 1 completed: Found address ID {AddressId}", result.AddressId);

            // Step 2: Validate that the address ID exists and get its details
            _logger.LogInformation("Step 2: Validating address ID {AddressId}", result.AddressId);
            var addressDetailsResponse = await ValidateAddressIdAsync(result.AddressId);

            if (!addressDetailsResponse.Success || !addressDetailsResponse.Exists)
            {
                result.AddressIdValid = false;
                result.Success = false;
                result.Message = "Address ID exists but cannot be validated";
                result.Error = addressDetailsResponse.Error;
                return result;
            }

            result.AddressIdValid = true;
            result.ValidatedAddressDetails = addressDetailsResponse;
            _logger.LogInformation("Step 2 completed: Address ID {AddressId} is valid", result.AddressId);

            // Step 3: Compare original address details with validated details
            _logger.LogInformation("Step 3: Comparing original and validated address details");
            result.AddressDetailsMatch = CompareAddressDetails(
                customerId, postalCode, street, city, countryId,
                addressDetailsResponse);

            result.Success = true;
            result.Message = result.AddressDetailsMatch
                ? "Address validation successful - all details match"
                : "Address validation successful - but some details differ";

            _logger.LogInformation("Bidirectional validation completed successfully for address ID {AddressId}", result.AddressId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred during bidirectional address validation");
            result.Success = false;
            result.Error = $"Exception: {ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// Compares original address details with validated address details from CERM API
    /// </summary>
    /// <param name="originalCustomerId">Original customer ID</param>
    /// <param name="originalPostalCode">Original postal code</param>
    /// <param name="originalStreet">Original street</param>
    /// <param name="originalCity">Original city</param>
    /// <param name="originalCountryId">Original country ID</param>
    /// <param name="validatedDetails">Validated address details from CERM API</param>
    /// <returns>True if details match, false otherwise</returns>
    private bool CompareAddressDetails(
        string originalCustomerId, string originalPostalCode, string originalStreet,
        string originalCity, string originalCountryId, AddressDetailsResponse validatedDetails)
    {
        // Compare customer ID
        if (!string.Equals(originalCustomerId, validatedDetails.CustomerId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Customer ID mismatch: Original={Original}, Validated={Validated}",
                originalCustomerId, validatedDetails.CustomerId);
            return false;
        }

        // Compare postal code (remove spaces for comparison)
        var normalizedOriginalPostal = originalPostalCode?.Replace(" ", "").ToUpperInvariant() ?? "";
        var normalizedValidatedPostal = validatedDetails.PostalCode?.Replace(" ", "").ToUpperInvariant() ?? "";
        if (!string.Equals(normalizedOriginalPostal, normalizedValidatedPostal))
        {
            _logger.LogWarning("Postal code mismatch: Original={Original}, Validated={Validated}",
                originalPostalCode, validatedDetails.PostalCode);
            return false;
        }

        // Compare street (truncate to 40 chars as done in fetch request)
        var normalizedOriginalStreet = (originalStreet?.Length > 40 ? originalStreet.Substring(0, 40) : originalStreet)?.ToUpperInvariant() ?? "";
        var normalizedValidatedStreet = validatedDetails.Street?.ToUpperInvariant() ?? "";
        if (!string.Equals(normalizedOriginalStreet, normalizedValidatedStreet))
        {
            _logger.LogWarning("Street mismatch: Original={Original}, Validated={Validated}",
                originalStreet, validatedDetails.Street);
            return false;
        }

        // Compare city
        if (!string.Equals(originalCity, validatedDetails.City, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("City mismatch: Original={Original}, Validated={Validated}",
                originalCity, validatedDetails.City);
            return false;
        }

        // Compare country
        if (!string.Equals(originalCountryId, validatedDetails.Country, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Country mismatch: Original={Original}, Validated={Validated}",
                originalCountryId, validatedDetails.Country);
            return false;
        }

        _logger.LogInformation("All address details match successfully");
        return true;
    }

    /// <summary>
    /// Helper method to send a product JSON payload to the CERM API
    /// </summary>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="token">The authentication token</param>
    /// <param name="hostHeader">The host header value</param>
    /// <param name="jsonPayload">The JSON payload</param>
    /// <returns>A response containing the product ID</returns>
    private async Task<ProductIdResponse> SendProductJsonAsync(string endpoint, Models.TokenResponse token, string hostHeader, string jsonPayload)
    {
        // Create an explicit HttpRequestMessage for better control
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        // Add headers
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        httpRequest.Headers.Add("Host", hostHeader);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Send the request
        var response = await _httpClient.SendAsync(httpRequest);

        // Check if the request was successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create product: {StatusCode} - {Content}", response.StatusCode, errorContent);

            // Return a failed response
            return new ProductIdResponse
            {
                Success = false,
                Error = $"Failed to create product: {response.StatusCode} - {errorContent}"
            };
        }

        // Parse the response
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Create product response: {Response}", responseContent);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(responseContent);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("Data", out JsonElement data) &&
                data.TryGetProperty("Id", out JsonElement id))
            {
                string productId = id.GetString() ?? string.Empty;

                var productResponse = new ProductIdResponse
                {
                    ProductId = productId,
                    Success = !string.IsNullOrEmpty(productId)
                };

                _logger.LogInformation("Successfully created product in CERM API: {ProductId}", productId);
                return productResponse;
            }
            else
            {
                _logger.LogError("Failed to extract product ID from response: {Response}", responseContent);
                return new ProductIdResponse
                {
                    Success = false,
                    Error = $"Failed to extract product ID from response: {responseContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing create product response: {Message}", ex.Message);
            return new ProductIdResponse
            {
                Success = false,
                Error = $"Error parsing create product response: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Helper method to send a sales order JSON payload to the CERM API
    /// </summary>
    /// <param name="endpoint">The API endpoint</param>
    /// <param name="token">The authentication token</param>
    /// <param name="hostHeader">The host header value</param>
    /// <param name="jsonPayload">The JSON payload</param>
    /// <returns>A response containing the sales order ID</returns>
    private async Task<SalesOrderIdResponse> SendSalesOrderJsonAsync(string endpoint, Models.TokenResponse token, string hostHeader, string jsonPayload)
    {
        // Create an explicit HttpRequestMessage for better control
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
        };

        // Add headers
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        httpRequest.Headers.Add("Host", hostHeader);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        // Send the request
        var response = await _httpClient.SendAsync(httpRequest);

        // Check if the request was successful
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create sales order: {StatusCode} - {Content}", response.StatusCode, errorContent);

            // Return a failed response
            return new SalesOrderIdResponse
            {
                Success = false,
                Error = $"Failed to create sales order: {response.StatusCode} - {errorContent}"
            };
        }

        // Parse the response
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogDebug("Create sales order response: {Response}", responseContent);

        try
        {
            using JsonDocument doc = JsonDocument.Parse(responseContent);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("Data", out JsonElement data) &&
                data.TryGetProperty("Id", out JsonElement id))
            {
                string salesOrderId = id.GetString() ?? string.Empty;

                var salesOrderResponse = new SalesOrderIdResponse
                {
                    SalesOrderId = salesOrderId,
                    Success = !string.IsNullOrEmpty(salesOrderId)
                };

                _logger.LogInformation("Successfully created sales order in CERM API: {SalesOrderId}", salesOrderId);
                return salesOrderResponse;
            }
            else
            {
                _logger.LogError("Failed to extract sales order ID from response: {Response}", responseContent);
                return new SalesOrderIdResponse
                {
                    Success = false,
                    Error = $"Failed to extract sales order ID from response: {responseContent}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing create sales order response: {Message}", ex.Message);
            return new SalesOrderIdResponse
            {
                Success = false,
                Error = $"Error parsing create sales order response: {ex.Message}"
            };
        }
    }
}
