using System.Windows;
using Microsoft.Extensions.Options;
using Syncfusion.SfSkinManager;
using Syncfusion.Themes.FluentLight.WPF;
using Syncfusion.Themes.Windows11Light.WPF;
using WPFCore.App.Configuration;

namespace WPFCore.App.Shared.Themes;

/// <summary>
/// Load Syncfusion FluentLight theme runtime và áp dụng cho <see cref="Window"/>.
/// Singleton — share qua DI.
/// </summary>
public sealed class FluentLightThemeLoader
{
    private readonly SyncfusionOptions _options;

    public FluentLightThemeLoader(IOptions<SyncfusionOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
    }

    public void ApplyTo(DependencyObject element)
    {
        ArgumentNullException.ThrowIfNull(element);
        // Syncfusion 25.x: Theme(string themeName) constructor — supported themes: FluentLight, MaterialLight, MaterialDark, FluentDark, Office2019White, Office2019Black, Windows11Light, Windows11Dark
        SfSkinManager.ApplyStylesOnApplication = true;
        SfSkinManager.SetTheme(element, new Theme(_options.Theme));
    }
}
