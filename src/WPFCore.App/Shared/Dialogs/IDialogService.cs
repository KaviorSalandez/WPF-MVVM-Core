namespace WPFCore.App.Shared.Dialogs;

/// <summary>
/// Dialog abstraction. Implementation mặc định dùng Syncfusion MessageBox;
/// có thể mở rộng để host custom UserControl dialogs (sẽ thêm ở wave sau).
/// </summary>
public interface IDialogService
{
    /// <summary>Hiển thị message thông tin (OK only).</summary>
    Task ShowMessageAsync(string title, string message, CancellationToken cancellationToken = default);

    /// <summary>Hiển thị hộp xác nhận Yes/No. Trả về <c>true</c> khi user chọn Yes.</summary>
    Task<bool> ShowConfirmationAsync(string title, string message, CancellationToken cancellationToken = default);

    /// <summary>Hiển thị lỗi (icon Error). Có thể truyền kèm <see cref="Exception"/> để hiện stack trace.</summary>
    Task ShowErrorAsync(string title, string message, Exception? exception = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Hiển thị ViewModel dưới dạng dialog popup modal.
    /// Trả về <c>true</c> nếu dialog kết thúc thành công (vd: user bấm Lưu),
    /// <c>false</c>/<c>null</c> nếu hủy bỏ.
    /// </summary>
    Task<bool?> ShowDialogAsync<TViewModel>(object? parameter = null) where TViewModel : class;
}
