using System.Collections;
using System.ComponentModel;
using System.Reflection;
using FluentValidation;

namespace WPFCore.App.Shared.Validation;

/// <summary>
/// Bridge giữa <see cref="FluentValidation.IValidator{T}"/> và
/// <see cref="INotifyDataErrorInfo"/> để WPF binding tự hiển thị lỗi validation.
/// </summary>
/// <remarks>
/// Target cần implement <see cref="INotifyDataErrorInfo"/> và có property
/// <c>Errors</c> (thường là <c>Dictionary&lt;string, List&lt;ValidationFailure&gt;&gt;</c>)
/// để adapter có thể raise <c>ErrorsChanged</c> qua reflection.
/// </remarks>
public static class NotifyDataErrorAdapter
{
    /// <summary>
    /// Validate <paramref name="instance"/>, raise <c>ErrorsChanged</c> cho từng property bị lỗi
    /// và cập nhật <c>HasErrors</c>. Trả về <c>true</c> nếu hợp lệ.
    /// </summary>
    public static async Task<bool> ValidateAsync<T>(
        IValidator<T> validator,
        T instance,
        INotifyDataErrorInfo target,
        Action onHasErrorsChanged,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(onHasErrorsChanged);

        var result = await validator.ValidateAsync(instance, cancellationToken).ConfigureAwait(false);

        var errorsChangedEvent = target.GetType().GetEvent("ErrorsChanged")
            ?? throw new InvalidOperationException(
                "Target không implement INotifyDataErrorInfo.ErrorsChanged.");

        foreach (var errorGroup in result.Errors.GroupBy(e => e.PropertyName))
        {
            RaiseErrorsChanged(target, errorsChangedEvent, errorGroup.Key);
        }

        SetHasErrors(target, !result.IsValid);
        onHasErrorsChanged.Invoke();

        return result.IsValid;
    }

    private static void RaiseErrorsChanged(object target, EventInfo errorsChangedEvent, string propertyName)
    {
        // Events are backed by delegate fields with the same name. Retrieve the field
        // (private in most classes) and read the underlying multicast delegate.
        var field = target.GetType().GetField(
            errorsChangedEvent.Name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        var handler = field?.GetValue(target) as EventHandler<DataErrorsChangedEventArgs>;
        handler?.Invoke(target, new DataErrorsChangedEventArgs(propertyName));
    }

    private static void SetHasErrors(object target, bool hasErrors)
    {
        // INotifyDataErrorInfo.HasErrors chỉ có getter; một số base class expose setter protected
        // (ví dụ Prism BindableBase, CommunityToolkit ObservableValidator). Tìm cả public lẫn non-public.
        var prop = target.GetType().GetProperty(
            "HasErrors",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        prop?.SetValue(target, hasErrors);
    }

    /// <summary>Lấy errors cho property (helper dùng cho INotifyDataErrorInfo.GetErrors).</summary>
    public static IEnumerable GetErrorsFor(INotifyDataErrorInfo target, string? propertyName)
    {
        ArgumentNullException.ThrowIfNull(target);
        return target.GetErrors(propertyName);
    }
}
