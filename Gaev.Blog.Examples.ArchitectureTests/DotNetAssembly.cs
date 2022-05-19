using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Gaev.Blog.Examples;

public class DotNetAssembly
{
    private DotNetAssembly(AssemblyDefinition assembly)
        => MonoCecilAssembly = assembly;

    public AssemblyDefinition MonoCecilAssembly { get; }
    public HashSet<DotNetAssembly> Dependencies { get; } = new();
    public HashSet<DotNetAssembly> BackwardsDependencies { get; } = new();

    public string FullName
        => MonoCecilAssembly.Name?.FullName ?? "";

    public string Name
        => MonoCecilAssembly.Name?.Name ?? "";

    public override int GetHashCode()
        => FullName.GetHashCode();

    public override string ToString()
        => Name;

    public override bool Equals(object obj)
        => obj is DotNetAssembly other && other.FullName.Equals(FullName);

    public static List<DotNetAssembly> LoadAll()
    {
        var entryPointLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
        var entryPoint = new DotNetAssembly(AssemblyDefinition.ReadAssembly(entryPointLocation));
        var allProjects = new Dictionary<string, DotNetAssembly>();
        LoadDependencies(entryPoint, allProjects);
        return allProjects.Values.ToList();
    }

    private static void LoadDependencies(DotNetAssembly parent, Dictionary<string, DotNetAssembly> allProjects)
    {
        allProjects[parent.FullName] = parent;
        if (parent.Dependencies.Any()) return;
        var dependencies = parent.MonoCecilAssembly.Modules.SelectMany(module => module
            .AssemblyReferences
            .Select(reference => allProjects.TryGetValue(reference.FullName, out var dependency)
                ? dependency
                : ResolveReference(module, reference))
            .Where(dependency => dependency != null)
        );
        foreach (var dependency in dependencies)
        {
            parent.Dependencies.Add(dependency);
            dependency.BackwardsDependencies.Add(parent);
            LoadDependencies(dependency, allProjects);
        }
    }

    private static DotNetAssembly ResolveReference(ModuleDefinition module, AssemblyNameReference assembly)
    {
        try
        {
            return new DotNetAssembly(module.AssemblyResolver.Resolve(assembly));
        }
        catch (AssemblyResolutionException)
        {
            return null;
        }
    }
}
