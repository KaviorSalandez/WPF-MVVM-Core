using FluentValidation.Results;

namespace WPFCore.App.Shared.Validation;

/// <summary>
/// Extension methods tiện ích cho <see cref="ValidationResult"/>.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>Chuyển lỗi sang <c>Dictionary&lt;propertyName, errorMessages[]&gt;</c> để binding WPF.</summary>
    public static IReadOnlyDictionary<string, string[]> ToErrorDictionary(this ValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }

    /// <summary>Nối tất cả lỗi thành một dòng duy nhất, phân cách bằng <c>;</c>.</summary>
    public static string ToSingleLineMessage(this ValidationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return string.Join("; ", result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
    }
}
