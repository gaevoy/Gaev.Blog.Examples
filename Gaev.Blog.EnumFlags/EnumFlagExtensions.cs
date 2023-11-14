using System.Linq.Expressions;

namespace Gaev.Blog.EnumFlags;

public static class EnumFlagExtensions
{
    public static TEnum SetFlag<TEnum>(this TEnum value, TEnum flag, bool state) where TEnum : Enum
    {
        // non-boxing conversion
        var left = Caster<TEnum, UInt64>.Cast(value);
        var right = Caster<TEnum, UInt64>.Cast(flag);
        var result = state
            ? left | right
            : left & ~right;
        return Caster<UInt64, TEnum>.Cast(result);
    }

    public static TEnum RaiseFlag<TEnum>(this TEnum value, TEnum flag) where TEnum : Enum
        => value.SetFlag(flag, true);

    public static TEnum LowerFlag<TEnum>(this TEnum value, TEnum flag) where TEnum : Enum
        => value.SetFlag(flag, false);

    /// <summary>
    /// C# non-boxing conversion of enum to numeric and back. Based on https://stackoverflow.com/a/23391746
    /// </summary>
    private static class Caster<TSource, TTarget>
    {
        public static readonly Func<TSource, TTarget> Cast = CreateConvertMethod();

        private static Func<TSource, TTarget> CreateConvertMethod()
        {
            var p = Expression.Parameter(typeof(TSource));
            var c = Expression.ConvertChecked(p, typeof(TTarget));
            return Expression.Lambda<Func<TSource, TTarget>>(c, p).Compile();
        }
    }
}
