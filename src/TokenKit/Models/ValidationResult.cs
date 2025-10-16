// Models/ValidationResult.cs
namespace TokenKit.Models;

public record ValidationResult(bool IsValid, string Message)
{
    public static ValidationResult Valid() => new(true, "OK");
    public static ValidationResult Invalid(string message) => new(false, message);
}

