using NUnit.Framework;

namespace Gaev.Blog;

[Parallelizable, Category("OtherParallelizable")]
public class OtherParallelizableTests
{
    [Test]
    public Task OtherTest()
        => Task.Delay(2_000);
}
