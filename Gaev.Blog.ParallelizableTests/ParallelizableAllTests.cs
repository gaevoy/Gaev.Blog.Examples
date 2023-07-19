using NUnit.Framework;

namespace Gaev.Blog;

// dotnet test --filter:"Category=ParallelizableAll"
// dotnet test --filter:"Category=ParallelizableAll|Category=Other"
// dotnet test --filter:"Category=ParallelizableAll|Category=OtherParallelizable"

[Parallelizable(ParallelScope.All), Category("ParallelizableAll")]
public class ParallelizableAllTests
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
