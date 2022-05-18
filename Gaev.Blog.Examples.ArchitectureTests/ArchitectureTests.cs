using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using static Gaev.Blog.Examples.Conventions;

namespace Gaev.Blog.Examples;

public class ArchitectureTests
{
    static readonly List<DotNetAssembly> AllAssemblies
        = DotNetAssembly.LoadAll();

    static IEnumerable<DotNetAssembly> MyAppProjects
        => AllAssemblies.Where(IsMyApp).Where(IsNotTest);

    static IEnumerable<DotNetAssembly> Contracts
        => MyAppProjects.Where(IsContract);

    static IEnumerable<DotNetAssembly> Implementations
        => MyAppProjects.Where(IsImplementation);

    [TestCaseSource(nameof(MyAppProjects))]
    public void Project_should_be_NetStandard(DotNetAssembly it)
        => it.IsDotNetStandard()
            .Should().BeTrue();

    [TestCaseSource(nameof(Contracts))]
    public void Contract_should_not_reference_contract(DotNetAssembly my)
        => my.Dependencies.Where(IsContract)
            .Should().BeEmpty();

    [TestCaseSource(nameof(Contracts))]
    public void Contract_should_not_have_other_dependencies(DotNetAssembly my)
        => my.Dependencies.Where(IsNotSystem)
            .Should().BeEmpty();

    [TestCaseSource(nameof(Contracts))]
    public void Contract_should_not_reference_implementation(DotNetAssembly my)
        => my.Dependencies.Where(IsImplementation)
            .Should().BeEmpty();

    [TestCaseSource(nameof(Implementations))]
    public void Implementation_should_not_reference_implementation(DotNetAssembly my)
        => my.Dependencies.Where(IsImplementation)
            .Should().BeEmpty();

    [TestCaseSource(nameof(Implementations))]
    public void Implementation_should_reference_DI(DotNetAssembly my)
        => my.Dependencies.Where(e => e.Name.StartsWith("Microsoft.Extensions.DependencyInjection"))
            .Should().NotBeEmpty();

    void CompilerHint()
    {
        _ = typeof(Shell.Bootstrap).Assembly;
    }
}

public static class Conventions
{
    public static bool IsMyApp(DotNetAssembly project)
        => project.Name.StartsWith("Gaev");

    public static bool IsNotMyApp(DotNetAssembly project)
        => !IsMyApp(project);

    public static bool IsTest(DotNetAssembly project)
        => IsMyApp(project) && project.Name.EndsWith("Tests");

    public static bool IsNotTest(DotNetAssembly project)
        => !IsTest(project);

    public static bool IsShell(DotNetAssembly project)
        => IsMyApp(project) && project.Name.EndsWith(".Shell");

    public static bool IsContract(DotNetAssembly project)
        => IsMyApp(project) && project.Name.EndsWith(".Api");

    public static bool IsImplementation(DotNetAssembly project)
        => IsMyApp(project) && !IsShell(project) && !IsContract(project) && !IsTest(project);

    public static bool IsSystem(DotNetAssembly assembly)
        => assembly.Name == "netstandard";

    public static bool IsNotSystem(DotNetAssembly assembly)
        => !IsSystem(assembly);
}