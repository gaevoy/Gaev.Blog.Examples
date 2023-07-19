using NUnit.Framework;

namespace Gaev.Blog;

// dotnet test --filter:"Category=ParallelizableAllPitfalls"

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All), Category("ParallelizableAllPitfalls")]
public class ParallelizableAllTestsPitfalls
{
    private int _state = 0;

    [SetUp]
    public void Setup()
        => WriteLine($"Setup   	State: {_state}	Instance: {GetHashCode()}");

    [TearDown]
    public void TearDown()
        => WriteLine($"TearDown	State: {_state}	Instance: {GetHashCode()}");

    [Test]
    public Task Test1()
        => KindOfTest("Test1");

    [Test]
    public Task Test2()
        => KindOfTest("Test2");

    [Test]
    public Task Test3()
        => KindOfTest("Test3");

    [Test]
    public Task Test4()
        => KindOfTest("Test4");

    [Test]
    public Task Test5()
        => KindOfTest("Test5");

    private async Task KindOfTest(string testName)
    {
        var initial = _state;
        _state++;
        var changed = _state;
        await Task.Delay(1_000);
        WriteLine($"{testName}	State: {initial}->{changed}->{_state}	Instance: {GetHashCode()}");
    }

    private void WriteLine(string message)
    {
        var position = Interlocked.Add(ref _currentOrder, 1);
        Console.WriteLine($"{position:00}. {message}");
    }

    private static int _currentOrder = 0;
}
