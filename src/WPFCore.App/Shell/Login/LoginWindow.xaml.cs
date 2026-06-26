using System.Windows;
using System.Windows.Input;

namespace WPFCore.App.Shell.Login;

/// <summary>
/// Code-behind cho <see cref="LoginWindow"/>. Inject <see cref="LoginViewModel"/> từ DI,
/// wire password box (không binding trực tiếp vì bảo mật), xử lý drag và close.
/// </summary>
public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        _viewModel = viewModel;
        DataContext = _viewModel;

        InitializeComponent();

        // Wire password box → ViewModel (PasswordBox.Password không bindable)
        PasswordBox.PasswordChanged += (_, _) =>
        {
            _viewModel.SetPassword(PasswordBox.Password);
            UpdatePasswordWatermark();
        };

        // Cập nhật watermark khi focus đổi (ẩn khi đang nhập, hiện lại khi rời đi mà còn trống)
        PasswordBox.GotKeyboardFocus += (_, _) => UpdatePasswordWatermark();
        PasswordBox.LostKeyboardFocus += (_, _) => UpdatePasswordWatermark();

        // Wire Enter key trong PasswordBox → kích hoạt LoginCommand
        PasswordBox.KeyDown += PasswordBox_KeyDown;

        // Khi đăng nhập thành công → close dialog với result = true
        _viewModel.LoginSucceeded += OnLoginSucceeded;

        // Trạng thái ban đầu: ô trống, chưa focus → hiện watermark
        UpdatePasswordWatermark();
    }

    /// <summary>
    /// Hiện watermark mật khẩu khi ô trống VÀ chưa được focus; ẩn khi đang nhập hoặc đã có ký tự.
    /// </summary>
    private void UpdatePasswordWatermark()
    {
        PasswordWatermark.Visibility =
            PasswordBox.Password.Length == 0 && !PasswordBox.IsKeyboardFocused
                ? Visibility.Visible
                : Visibility.Collapsed;
    }

    // ──────────────────────────────────────
    // Event handlers
    // ──────────────────────────────────────

    private void OnLoginSucceeded(object? sender, EventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return && _viewModel.LoginCommand.CanExecute(null))
        {
            _viewModel.LoginCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void UsernameBox_KeyDown(object sender, KeyEventArgs e)
    {
        // Enter trong ô Username → di chuyển focus xuống ô Password
        if (e.Key == Key.Return)
        {
            PasswordBox.Focus();
            e.Handled = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Cho phép kéo cửa sổ không có chrome
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); }
            catch (InvalidOperationException)
            {
                // Bỏ qua nếu chuột không còn được nhấn (race condition hiếm gặp)
            }
        }
    }

    // ──────────────────────────────────────
    // Cleanup
    // ──────────────────────────────────────

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LoginSucceeded -= OnLoginSucceeded;
        base.OnClosed(e);
    }
}
