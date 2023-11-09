using System.Linq.Expressions;

namespace Gaev.Blog.EnumFlags;

public static class EnumFlagExtensions
{
    public static TEnum SetFlag<TEnum>(this TEnum value, TEnum flag, bool state) where TEnum : Enum
    {
        // non-boxing conversion based on https://stackoverflow.com/a/23391746
        var left = Caster<TEnum, UInt64>.Cast(value);
        var right = Caster<TEnum, UInt64>.Cast(flag);
        var result = state
            ? left | right
            : left & ~right;
        return Caster<ulong, TEnum>.Cast(result);
    }

    public static TEnum SetFlagWithBoxing<TEnum>(this TEnum value, TEnum flag, bool state) where TEnum : Enum
    {
        // conversion with boxing
        var left = Convert.ToUInt64(value);
        var right = Convert.ToUInt64(flag);
        var result = state
            ? left | right
            : left & ~right;
        return (TEnum)Convert.ChangeType(result, Enum.GetUnderlyingType(typeof(TEnum)));
    }

    public static TEnum RaiseFlag<TEnum>(this TEnum value, TEnum flag) where TEnum : Enum
        => value.SetFlag(flag, true);

    public static TEnum LowerFlag<TEnum>(this TEnum value, TEnum flag) where TEnum : Enum
        => value.SetFlag(flag, false);

    public static class Caster<TSource, TTarget>
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
