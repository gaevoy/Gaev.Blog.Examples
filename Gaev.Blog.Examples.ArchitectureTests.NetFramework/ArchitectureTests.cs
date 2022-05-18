using System.Linq;
using NUnit.Framework;

namespace Gaev.Blog.Examples;

public class ArchitectureTests
{
    [Test]
    public void Playground()
    {
        var assemblyTree = DotNetAssembly.LoadAll()
            .Where(e => e.FullName.StartsWith("Gaev."))
            .ToList();
        var sss = 123;
    }

    private void CompilerHint()
    {
        _ = typeof(Shell.Bootstrap).Assembly;
    }
}
