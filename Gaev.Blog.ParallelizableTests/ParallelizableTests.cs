using NUnit.Framework;

namespace Gaev.Blog;

// dotnet test --filter:"Category=Parallelizable"
// dotnet test --filter:"Category=Parallelizable|Category=Other"
// dotnet test --filter:"Category=Parallelizable|Category=OtherParallelizable"

[Parallelizable, Category("Parallelizable")]
public class ParallelizableTests
{
    [Test]
    public Task Test1()
        => Task.Delay(1_000);

    [Test]
    public Task Test2()
        => Task.Delay(1_000);

    [Test]
    public Task Test3()
        => Task.Delay(1_000);

    [Test]
    public Task Test4()
        => Task.Delay(1_000);

    [Test]
    public Task Test5()
        => Task.Delay(1_000);
}
