using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace WPFCore.App.Shared.Navigation;

/// <summary>
/// Convention-based view resolution. Tìm View theo ViewModel name:
/// <c>CustomerListViewModel</c> → <c>CustomerListView</c>. Cache kết quả để tránh reflection lặp lại.
/// </summary>
public static class ViewLocator
{
    private static readonly Dictionary<Type, Type> _cache = new();

    public static FrameworkElement Locate(Type viewModelType, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        ArgumentNullException.ThrowIfNull(services);

        if (_cache.TryGetValue(viewModelType, out var cachedViewType))
        {
            return (FrameworkElement)services.GetRequiredService(cachedViewType);
        }

        var vmName = viewModelType.Name;
        if (!vmName.EndsWith("ViewModel", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"ViewModel '{vmName}' không kết thúc bằng 'ViewModel'.");
        }

        var viewName = vmName[..^"ViewModel".Length] + "View";

        var viewType = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    return Array.Empty<Type>();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .FirstOrDefault(t => t.Name == viewName && typeof(FrameworkElement).IsAssignableFrom(t))
            ?? throw new InvalidOperationException(
                $"Không tìm thấy View '{viewName}' cho ViewModel '{vmName}'.");

        _cache[viewModelType] = viewType;
        return (FrameworkElement)services.GetRequiredService(viewType);
    }
}
