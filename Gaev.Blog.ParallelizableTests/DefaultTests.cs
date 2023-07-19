using NUnit.Framework;

namespace Gaev.Blog;

// dotnet test --filter:"Category=Default"
// dotnet test --filter:"Category=Default|Category=Other"

[Category("Default")]
public class DefaultTests
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
