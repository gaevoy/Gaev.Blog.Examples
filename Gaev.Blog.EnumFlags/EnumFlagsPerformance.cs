using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Gaev.Blog.EnumFlags;

[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser]
public class EnumFlagsPerformance
{
    private Pet _value = Pet.Cat | Pet.Dog;

    [Benchmark]
    public void RaiseFlag_Native() => _value = _value | Pet.Bird;

    [Benchmark]
    public void RaiseFlag_NonBoxing() => _value = _value.SetFlag(Pet.Bird, true);

    [Benchmark]
    public void RaiseFlag_Boxing() => _value = SetFlagWithBoxing(_value, Pet.Bird, true);

    [Benchmark]
    public void LowerFlag_Native() => _value = _value & ~Pet.Dog;

    [Benchmark]
    public void LowerFlag_NonBoxing() => _value = _value.SetFlag(Pet.Dog, false);

    [Benchmark]
    public void LowerFlag_Boxing() => _value = SetFlagWithBoxing(_value, Pet.Dog, false);

    private static TEnum SetFlagWithBoxing<TEnum>(TEnum value, TEnum flag, bool state) where TEnum : Enum
    {
        // conversion with boxing
        var left = Convert.ToUInt64(value);
        var right = Convert.ToUInt64(flag);
        var result = state
            ? left | right
            : left & ~right;
        return (TEnum)Convert.ChangeType(result, Enum.GetUnderlyingType(typeof(TEnum)));
    }
}
