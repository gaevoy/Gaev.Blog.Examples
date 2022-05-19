using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using PlantUml.Net;
using static Gaev.Blog.Examples.Conventions;

namespace Gaev.Blog.Examples;

public class ArchitectureTests
{
    static readonly List<DotNetAssembly> AllAssemblies
        = DotNetAssembly.LoadAll();

    public static IEnumerable<DotNetAssembly> AppProjects
        => AllAssemblies.Where(IsMyApp).Where(IsNotTest);

    [TestCaseSource(nameof(AppProjects))]
    public void App_project_should_be_NetStandard(DotNetAssembly it)
        => it.IsDotNetStandard()
            .Should().BeTrue();

    [TestCaseSource(nameof(AppProjects))]
    public void App_project_should_have_NetStandard_dependencies_only(DotNetAssembly my)
        => my.Dependencies
            .Should().OnlyContain(e => e.IsDotNetStandard());

    public static IEnumerable<DotNetAssembly> Contracts
        => AppProjects.Where(IsContract);

    [TestCaseSource(nameof(Contracts))]
    public void Contract_should_not_reference_contract(DotNetAssembly contract)
        => contract.Dependencies
            .Should().NotContain(e => IsContract(e));

    [TestCaseSource(nameof(Contracts))]
    public void Contract_should_not_have_any_dependencies(DotNetAssembly contract)
        => contract.Dependencies
            .Should().OnlyContain(e => IsSystem(e));

    [TestCaseSource(nameof(Contracts))]
    public void Contract_should_not_reference_implementation(DotNetAssembly contract)
        => contract.Dependencies
            .Should().NotContain(e => IsImplementation(e));

    public static IEnumerable<DotNetAssembly> Implementations
        => AppProjects.Where(IsImplementation);

    [TestCaseSource(nameof(Implementations))]
    public void Implementation_should_not_reference_implementation(DotNetAssembly implementation)
        => implementation.Dependencies
            .Should().NotContain(e => IsImplementation(e));

    [TestCaseSource(nameof(Implementations))]
    public void Implementation_should_reference_DI(DotNetAssembly implementation)
        => implementation.Dependencies
            .Should().Contain(e => e.Name.StartsWith("Microsoft.Extensions.DependencyInjection"));

    [TestCaseSource(nameof(AppProjects))]
    public void It_should_render_PlantUml_diagram(DotNetAssembly project)
    {
        var plantUmlCode = project.RenderPlantUmlDiagram(IsMyApp);
        var svgDiagramUrl = new RendererFactory()
            .CreateRenderer()
            .RenderAsUri(plantUmlCode, OutputFormat.Svg);
        Console.WriteLine($"{svgDiagramUrl}\n{plantUmlCode}");
    }

    private void CompilerHint()
    {
        // This should make compiler to include the following dependencies 
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
