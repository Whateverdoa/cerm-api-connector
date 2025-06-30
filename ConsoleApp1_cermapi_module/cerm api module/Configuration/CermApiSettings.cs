namespace aws_b2b_mod1.Configuration;

public class CermApiSettings
{
    public string Environment { get; set; } = "Test";

    // Test environment settings
    public CermEnvironmentSettings Test { get; set; } = new CermEnvironmentSettings
    {
        BaseUrl = "https://vilatest-api.cerm.be/",
        HostHeader = "vilatest-api.cerm.be"
    };

    // Production environment settings
    public CermEnvironmentSettings Production { get; set; } = new CermEnvironmentSettings
    {
        BaseUrl = "https://vila-api.cerm.be/",
        HostHeader = "vila-api.cerm.be"
    };

    // Authentication settings
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // API Endpoints
    public CermApiPaths Paths { get; set; } = new CermApiPaths();

    // Get the current environment settings
    public CermEnvironmentSettings GetCurrentEnvironment()
    {
        return Environment.ToLower() == "production" ? Production : Test;
    }

    // Get the base URL for the current environment
    public string GetBaseUrl()
    {
        return GetCurrentEnvironment().BaseUrl;
    }

    // Get the host header for the current environment
    public string GetHostHeader()
    {
        return GetCurrentEnvironment().HostHeader;
    }
}

public class CermEnvironmentSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string HostHeader { get; set; } = string.Empty;
}

public class CermApiPaths
{
    public string Token { get; set; } = "oauth/token";
    public string FetchAddressId { get; set; } = "custom-api/export/fetchaddressid";
    public string CreateAddress { get; set; } = "address-api/v1/addresses";
    public string FetchAddressById { get; set; } = "address-api/v1/addresses/{id}";
    public string FetchCalculationId { get; set; } = "custom-api/export/fetchcalculationid";
    public string CreateCalculation { get; set; } = "quote-api/v1/calculations";
    public string CreateQuote { get; set; } = "quote-api/v1/calculations";
    public string CreateProduct { get; set; } = "product-api/v1/calculations/{calculationId}/products";
    public string CreateSalesOrder { get; set; } = "sales-order-api/v1/customers/{customerId}/contacts/{contactId}/sales-orders/order";
}
