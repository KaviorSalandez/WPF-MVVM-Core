using System.Threading.Tasks;

namespace WPFCore.App.Data;

/// <summary>
/// Giao diện chuẩn cho các class sinh dữ liệu mẫu lúc khởi động ứng dụng (Runtime Seeding).
/// Thay thế cho việc dùng Reflection gọi bằng chuỗi (Magic String).
/// </summary>
public interface IModuleSeeder
{
    /// <summary>
    /// Chạy tiến trình sinh dữ liệu của Module.
    /// </summary>
    Task SeedAsync();
}
