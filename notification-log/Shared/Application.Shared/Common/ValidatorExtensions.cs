using FluentValidation;

namespace Application.Shared.Common;

public static class ValidatorExtensions
{
    public static async Task ValidateAndThrowAppAsync<T>(
        this IValidator<T> validator, T instance, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (result.IsValid) return;

        throw new AppValidationException(
            result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));
    }
}
