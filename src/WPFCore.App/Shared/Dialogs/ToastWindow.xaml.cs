using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WPFCore.App.Shared.Dialogs;

public partial class ToastWindow : Window
{
    private readonly DispatcherTimer _timer;

    public ToastWindow(string message, int durationMs = 3000)
    {
        InitializeComponent();
        MessageText.Text = message;

        // Position the window at the bottom center of the main window
        Loaded += OnLoaded;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var main = Application.Current.MainWindow;
        if (main != null && main.IsVisible)
        {
            // Lấy tọa độ tuyệt đối của MainWindow trên màn hình để tránh lỗi khi Maximize (Left bị âm)
            var locationFromScreen = main.PointToScreen(new Point(0, 0));
            
            // Dùng ActualWidth/ActualHeight của cả Main và Toast để căn giữa chính xác
            this.Left = locationFromScreen.X + (main.ActualWidth - this.ActualWidth) / 2;
            this.Top = locationFromScreen.Y + main.ActualHeight - this.ActualHeight - 20; 
            
            this.Owner = main;
        }
        else
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _timer.Stop();
        
        // Tạo hiệu ứng Fade Out trước khi đóng
        var fadeOutAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = new Duration(TimeSpan.FromMilliseconds(300))
        };
        
        fadeOutAnimation.Completed += (s, args) => this.Close();
        
        this.BeginAnimation(Window.OpacityProperty, fadeOutAnimation);
    }
}
