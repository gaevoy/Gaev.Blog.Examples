using BenchmarkDotNet.Attributes;

namespace Gaev.Blog.EnumFlags;

[MemoryDiagnoser]
public class EnumFlagsPerformance
{
    private Pet _value = Pet.Cat | Pet.Dog;

    [Benchmark]
    public void RaiseFlag_Native() => _value = _value | Pet.Bird;

    [Benchmark]
    public void RaiseFlag_NonBoxing() => _value = _value.SetFlag(Pet.Bird, true);

    [Benchmark]
    public void RaiseFlag_Boxing() => _value = _value.SetFlagWithBoxing(Pet.Bird, true);

    [Benchmark]
    public void LowerFlag_Native() => _value = _value & ~Pet.Dog;

    [Benchmark]
    public void LowerFlag_NonBoxing() => _value = _value.SetFlag(Pet.Dog, false);

    [Benchmark]
    public void LowerFlag_Boxing() => _value = _value.SetFlagWithBoxing(Pet.Dog, false);
}
