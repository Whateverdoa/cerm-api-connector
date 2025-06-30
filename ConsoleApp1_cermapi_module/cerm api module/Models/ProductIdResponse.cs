using System.Text.Json.Serialization;

namespace aws_b2b_mod1.Models;

public class ProductIdResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
}
