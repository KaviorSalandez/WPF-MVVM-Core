using System.Data;
using Dapper;

namespace WPFCore.App.Data;

/// <summary>
/// Dapper TypeHandler cho <see cref="DateOnly"/> (.NET 6+). Lưu trong DB dưới dạng TEXT theo ISO format
/// "yyyy-MM-dd" (compatible với cả SQLite TEXT và SQL Server DATE).
/// </summary>
/// <remarks>
/// Dapper không có sẵn support cho DateOnly (introduced in .NET 6). Handler này ánh xạ DateOnly ↔ string ISO.
/// Register trong SqliteConnectionFactory.CreateOpenConnectionAsync() qua <see cref="SqlMapper.AddTypeHandler{T}"/>.
/// </remarks>
public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        return value switch
        {
            DateTime dt => DateOnly.FromDateTime(dt),
            string s => DateOnly.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
            long l => DateOnly.FromDateTime(DateTime.UnixEpoch.AddTicks(l)),
            _ => throw new DataException($"Cannot convert {value?.GetType().FullName ?? "null"} to DateOnly")
        };
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// Dapper TypeHandler cho nullable DateOnly (DateOnly?).
/// </summary>
public sealed class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value)
    {
        if (value is null or DBNull) return null;
        return value switch
        {
            DateTime dt => DateOnly.FromDateTime(dt),
            string s => DateOnly.Parse(s, System.Globalization.CultureInfo.InvariantCulture),
            long l => DateOnly.FromDateTime(DateTime.UnixEpoch.AddTicks(l)),
            _ => throw new DataException($"Cannot convert {value?.GetType().FullName ?? "null"} to DateOnly?")
        };
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly? value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? (object)DBNull.Value;
    }
}
