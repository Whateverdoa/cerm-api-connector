using System.Text.Json.Serialization;

namespace aws_b2b_mod1.Models;

public class CalculationIdResponse
{
    [JsonPropertyName("calculationId")]
    public string CalculationId { get; set; } = string.Empty;
    
    [JsonIgnore]
    public bool Success { get; set; }
    
    [JsonIgnore]
    public string Error { get; set; } = string.Empty;
}
