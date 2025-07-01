using System.Text.Json.Serialization;

namespace CermApiConnector.Models;

public class AddressValidationResult
{
    /// <summary>
    /// Original address details provided for validation
    /// </summary>
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    [JsonPropertyName("street")]
    public string Street { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("countryId")]
    public string CountryId { get; set; } = string.Empty;

    /// <summary>
    /// Validation results
    /// </summary>
    [JsonPropertyName("addressId")]
    public string AddressId { get; set; } = string.Empty;

    [JsonPropertyName("addressIdFound")]
    public bool AddressIdFound { get; set; }

    [JsonPropertyName("addressIdValid")]
    public bool AddressIdValid { get; set; }

    [JsonPropertyName("addressDetailsMatch")]
    public bool AddressDetailsMatch { get; set; }

    [JsonPropertyName("validatedAddressDetails")]
    public AddressDetailsResponse? ValidatedAddressDetails { get; set; }

    /// <summary>
    /// Overall validation result
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Summary of validation steps performed
    /// </summary>
    [JsonPropertyName("validationSummary")]
    public string ValidationSummary =>
        $"AddressID Found: {AddressIdFound}, " +
        $"AddressID Valid: {AddressIdValid}, " +
        $"Details Match: {AddressDetailsMatch}, " +
        $"Overall Success: {Success}";
}
