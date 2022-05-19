using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using PlantUml.Net;

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
        => assembly.Name == "netstandard" || assembly.GetTargetFramework().StartsWith(".NETStandard");

    public static string RenderPlantUmlDiagram(this DotNetAssembly project, Func<DotNetAssembly, bool> condition)
    {
        var plantUmlCode = new StringBuilder();

        void Render(DotNetAssembly parent)
        {
            plantUmlCode.AppendLine($"[{parent}]");
            foreach (var dependency in parent.Dependencies.Where(condition))
            {
                plantUmlCode.AppendLine($"[{parent}] -up-> [{dependency}]");
                Render(dependency);
            }
        }

        Render(project);
        return $"@startuml\n\n{plantUmlCode}\n@enduml";
    }
}
