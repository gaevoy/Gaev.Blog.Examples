using NUnit.Framework;

namespace Gaev.Blog;

// dotnet test --filter:"Category=ParallelizableAllWithCases"

[Parallelizable(ParallelScope.All), Category("ParallelizableAllWithCases")]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ParallelizableAllTestCases
{
    private int _state;

    [SetUp]
    public void Setup()
    {
        var initial = _state;
        _state = 0;
        Console.WriteLine($"{GetPosition():00}. Setup	->	State: {initial}->{_state}	TestInstance: {GetHashCode()}");
    }

    [TearDown]
    public void TearDown()
    {
        Console.WriteLine($"{GetPosition():00}. TearDow	->	State: {_state}	TestInstance: {GetHashCode()}");
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public Task Test(int number)
        => KindOfTest($"Test({number})");

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
        Console.WriteLine(
            $"{GetPosition():00}. {testName}	->	State: {initial}->{changed}->{_state}	TestInstance: {GetHashCode()}");
    }

    private static int _currentOrder = 0;

    private static int GetPosition()
        => Interlocked.Add(ref _currentOrder, 1);
}
