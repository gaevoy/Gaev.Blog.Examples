using BenchmarkDotNet.Running;

namespace Gaev.Blog.EnumFlags;

public static class Program
{
    public static void Main(string[] _)
        => BenchmarkRunner.Run(typeof(Program).Assembly);
}
