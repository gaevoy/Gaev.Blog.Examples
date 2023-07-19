using NUnit.Framework;

namespace Gaev.Blog;

[Category("Other")]
public class OtherTests
{
    [Test]
    public Task OtherTest()
        => Task.Delay(2_000);
}
