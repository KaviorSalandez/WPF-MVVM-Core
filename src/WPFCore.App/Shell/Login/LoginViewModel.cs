using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WPFCore.App.Shared.ViewModels;

namespace WPFCore.App.Shell.Login;

/// <summary>
/// ViewModel cho màn hình đăng nhập. Xác thực thông tin đăng nhập cứng
/// (admin / 123123) và raise event <see cref="LoginSucceeded"/> khi thành công.
/// </summary>
public sealed partial class LoginViewModel : ViewModelBase
{
    private readonly ILogger<LoginViewModel> _logger;
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    /// <summary>Raised khi đăng nhập thành công. LoginWindow lắng nghe event này để close với DialogResult=true.</summary>
    public event EventHandler? LoginSucceeded;

    public LoginViewModel(ILogger<LoginViewModel> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        Title = "Đăng nhập";
    }

    /// <summary>
    /// Được gọi từ code-behind của LoginWindow mỗi khi PasswordBox thay đổi.
    /// Password không binding trực tiếp vì lý do bảo mật.
    /// </summary>
    public void SetPassword(string password)
    {
        _password = password ?? string.Empty;
        LoginCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private void Login()
    {
        HasError = false;
        ErrorMessage = null;

        // Hard-coded credentials — thay bằng DB/service trong production
        if (Username.Trim().Equals("admin", StringComparison.OrdinalIgnoreCase)
            && _password == "123123")
        {
            _logger.LogInformation("Đăng nhập thành công — người dùng: '{Username}'", Username.Trim());
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            _logger.LogWarning("Đăng nhập thất bại — username: '{Username}'", Username.Trim());
            ErrorMessage = "Tên đăng nhập hoặc mật khẩu không chính xác.";
            HasError = true;
        }
    }

    private bool CanLogin()
        => !string.IsNullOrWhiteSpace(Username) && _password.Length > 0;
}
