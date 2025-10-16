// Models/ModelSpec.cs
namespace TokenKit.Models;

public class ModelSpec
{
    public string Id { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public decimal InputPricePer1K { get; set; }
    public decimal OutputPricePer1K { get; set; }
    public string Encoding { get; set; } = string.Empty;
}
