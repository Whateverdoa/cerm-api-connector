using System.Text.Json.Serialization;

namespace CermApiConnector.Models;

public class QuoteIdResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("calculationId")]
    public string CalculationId { get; set; } = string.Empty;
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
}
