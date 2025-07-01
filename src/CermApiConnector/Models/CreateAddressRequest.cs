using System.Text.Json.Serialization;

namespace CermApiConnector.Models;

public class CreateAddressRequest
{
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("street")]
    public string Street { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("countryId")]
    public string CountryId { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("isDeliveryAddress")]
    public bool IsDeliveryAddress { get; set; } = true;

    [JsonPropertyName("isInvoiceAddress")]
    public bool IsInvoiceAddress { get; set; } = false;

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("contacts")]
    public List<object> Contacts { get; set; } = new List<object>();
}
