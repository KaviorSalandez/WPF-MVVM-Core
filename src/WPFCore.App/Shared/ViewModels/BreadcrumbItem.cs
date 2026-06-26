using System.Windows.Input;

namespace WPFCore.App.Shared.ViewModels;

/// <summary>
/// Model for a breadcrumb navigation node.
/// </summary>
public sealed class BreadcrumbItem
{
    /// <summary>
    /// The display title of the breadcrumb.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The ViewModel instance associated with this breadcrumb node.
    /// </summary>
    public object ViewModel { get; init; } = null!;

    /// <summary>
    /// The command to execute when the breadcrumb is clicked.
    /// </summary>
    public ICommand NavigateCommand { get; init; } = null!;
}
