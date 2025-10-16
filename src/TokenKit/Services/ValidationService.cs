using TokenKit.Models;

namespace TokenKit.Services;

public class ValidationService
{
    public ValidationResult Validate(ModelSpec model, int tokenCount)
    {
        return tokenCount > model.MaxTokens
            ? ValidationResult.Invalid($"Prompt exceeds {model.MaxTokens} tokens for {model.Id}")
            : ValidationResult.Valid();
    }
}
