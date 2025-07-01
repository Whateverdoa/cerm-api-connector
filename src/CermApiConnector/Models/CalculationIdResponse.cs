using System.Text.Json.Serialization;

namespace CermApiConnector.Models;

public class CalculationIdResponse
{
    [JsonPropertyName("calculationId")]
    public string CalculationId { get; set; } = string.Empty;

    [JsonIgnore]
    public bool Success { get; set; }

    [JsonIgnore]
    public string Error { get; set; } = string.Empty;
}
