using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using WPFCore.App.Shared.Navigation;

namespace WPFCore.App.Shell;

/// <summary>
/// Shell main window. Inject <see cref="FrameNavigationService"/> từ DI (đã đăng ký singleton
/// ở Bootstrap), tạo <see cref="Frame"/> runtime và gắn vào <see cref="INavigationService"/>
/// ngay sau <see cref="InitializeComponent"/>.
/// </summary>
public partial class MainWindow : Window
{
    private readonly FrameNavigationService _navigation;

    public MainWindow(FrameNavigationService navigation, ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        _navigation = navigation;

        // ContentControl trong XAML được thay bằng Frame để INavigationService điều khiển.
        var frame = new Frame
        {
            NavigationUIVisibility = NavigationUIVisibility.Hidden
        };
        NavigationHost.Content = frame;

        _navigation.Attach(frame);
    }
}
