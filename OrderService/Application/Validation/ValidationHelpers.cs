using FluentValidation.Results;

namespace Application.Validation;

public static class ValidationHelpers
{
    public static IDictionary<string, string[]> ToValidationProblemErrors(ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(e => string.IsNullOrWhiteSpace(e.PropertyName) ? "" : e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).Distinct().ToArray());
    }
}
