using System.Linq;
using System.Runtime.Versioning;

namespace Gaev.Blog.Examples;

public static class DotNetAssemblyExt
{
    public static string GetTargetFramework(this DotNetAssembly assembly)
        => assembly.MonoCecilAssembly.CustomAttributes
            .Where(e => e.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName)
            .SelectMany(e => e.ConstructorArguments)
            .Select(e => e.Value as string)
            .FirstOrDefault() ?? "";

    public static bool IsDotNetStandard(this DotNetAssembly assembly)
        => assembly.GetTargetFramework().StartsWith(".NETStandard");
}