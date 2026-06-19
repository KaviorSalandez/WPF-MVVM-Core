namespace WPFCore.App.Shared.Dialogs;

/// <summary>
/// Interface cho ViewModel muốn được hiển thị dưới dạng dialog popup.
/// Khi ViewModel implement interface này, <see cref="SyncfusionDialogService"/> sẽ
/// lắng nghe event <see cref="RequestClose"/> để tự động đóng cửa sổ dialog.
/// </summary>
public interface IDialogAware
{
    /// <summary>Tiêu đề hiển thị trên thanh title của dialog window.</summary>
    string? DialogTitle { get; }

    /// <summary>
    /// Fired khi ViewModel muốn đóng dialog.
    /// Tham số bool: <c>true</c> = thao tác thành công (vd: đã lưu),
    /// <c>false</c>/<c>null</c> = hủy bỏ.
    /// </summary>
    event Action<bool?>? RequestClose;
}
