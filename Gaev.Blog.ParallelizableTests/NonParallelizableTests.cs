using NUnit.Framework;

namespace Gaev.Blog;

// dotnet test --filter:"Category=NonParallelizable"
// dotnet test --filter:"Category=NonParallelizable|Category=Other"

[NonParallelizable, Category("NonParallelizable")]
public class NonParallelizableTests
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
