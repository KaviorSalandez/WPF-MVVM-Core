using System.Runtime.CompilerServices;
using Dapper;

namespace WPFCore.App.Data;

/// <summary>
/// Auto-registers Dapper TypeHandlers khi assembly được load (cả production và test app).
/// Sử dụng <see cref="ModuleInitializerAttribute"/> — chạy exactly-once per AppDomain khi assembly load.
/// </summary>
internal static class DapperBootstrap
{
    [ModuleInitializer]
    public static void Initialize()
    {
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
    }
}
