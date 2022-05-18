using NUnit.Framework;

namespace Gaev.Blog.Examples;

public class ArchitectureTests
{
    [Test]
    public void Playground()
    {
        var assemblyTree = DotNetAssembly.LoadAll();
    }
}
