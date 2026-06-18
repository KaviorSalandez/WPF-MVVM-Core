using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace WPFCore.Tests.Shared;

/// <summary>
/// Base class cho tất cả test fixtures. Cung cấp helpers để tạo Moq mocks
/// (strict/loose) và <see cref="ILogger{T}"/> an toàn cho unit tests.
/// </summary>
public abstract class TestBase
{
    /// <summary>Tạo Moq <see cref="Mock{T}"/> ở chế độ <see cref="MockBehavior.Strict"/> — mọi call chưa setup sẽ throw.</summary>
    protected static Mock<T> CreateMock<T>() where T : class => new(MockBehavior.Strict);

    /// <summary>Tạo Moq <see cref="Mock{T}"/> ở chế độ <see cref="MockBehavior.Loose"/> — call chưa setup trả về default.</summary>
    protected static Mock<T> CreateLooseMock<T>() where T : class => new(MockBehavior.Loose);

    /// <summary>Tạo <see cref="ILogger{T}"/> no-op dùng <see cref="NullLogger{T}.Instance"/> — không ghi log ra ngoài.</summary>
    protected static ILogger<T> CreateLogger<T>()
    {
        return NullLogger<T>.Instance;
    }

    /// <summary>Mốc thời gian cố định dùng cho các test deterministic (2026-06-18 08:30 UTC).</summary>
    protected static DateTime FixedNow => new(2026, 6, 18, 8, 30, 0, DateTimeKind.Utc);
}
