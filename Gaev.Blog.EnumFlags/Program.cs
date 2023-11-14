using BenchmarkDotNet.Running;

namespace Gaev.Blog.EnumFlags;

public static class Program
{
    public static void Main(string[] _)
        => BenchmarkRunner.Run(typeof(Program).Assembly);
}

/*
// * Summary *

BenchmarkDotNet v0.13.10, Windows 10 (10.0.19045.3570/22H2/2022Update)
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.401
  [Host]             : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2
  .NET 7.0           : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2
  .NET Framework 4.8 : .NET Framework 4.8.1 (4.8.9181.0), X64 RyuJIT VectorSize=256

| Method              | Job                | Runtime            | Mean        | Error     | StdDev    | Gen0   | Allocated |
|-------------------- |------------------- |------------------- |------------:|----------:|----------:|-------:|----------:|
| RaiseFlag_Native    | .NET 7.0           | .NET 7.0           |   0.3910 ns | 0.0194 ns | 0.0181 ns |      - |         - |
| RaiseFlag_NonBoxing | .NET 7.0           | .NET 7.0           |   2.7012 ns | 0.0119 ns | 0.0111 ns |      - |         - |
| RaiseFlag_Boxing    | .NET 7.0           | .NET 7.0           |  52.0814 ns | 0.6821 ns | 0.6380 ns | 0.0114 |     144 B |
| LowerFlag_Native    | .NET 7.0           | .NET 7.0           |   0.4116 ns | 0.0109 ns | 0.0102 ns |      - |         - |
| LowerFlag_NonBoxing | .NET 7.0           | .NET 7.0           |   2.6383 ns | 0.0146 ns | 0.0137 ns |      - |         - |
| LowerFlag_Boxing    | .NET 7.0           | .NET 7.0           |  51.5234 ns | 0.3467 ns | 0.2895 ns | 0.0114 |     144 B |
| RaiseFlag_Native    | .NET Framework 4.8 | .NET Framework 4.8 |   0.3665 ns | 0.0234 ns | 0.0269 ns |      - |         - |
| RaiseFlag_NonBoxing | .NET Framework 4.8 | .NET Framework 4.8 |   3.5727 ns | 0.0178 ns | 0.0167 ns |      - |         - |
| RaiseFlag_Boxing    | .NET Framework 4.8 | .NET Framework 4.8 | 100.7910 ns | 0.2107 ns | 0.1759 ns | 0.0229 |     144 B |
| LowerFlag_Native    | .NET Framework 4.8 | .NET Framework 4.8 |   0.3292 ns | 0.0305 ns | 0.0327 ns |      - |         - |
| LowerFlag_NonBoxing | .NET Framework 4.8 | .NET Framework 4.8 |   3.8718 ns | 0.0203 ns | 0.0170 ns |      - |         - |
| LowerFlag_Boxing    | .NET Framework 4.8 | .NET Framework 4.8 | 103.2550 ns | 2.0824 ns | 2.3981 ns | 0.0229 |     144 B |
 */
