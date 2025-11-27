using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator - perf

namespace DecSm.Atom.SourceGenerators;

[Generator]
public class GenerateSolutionModelSourceGenerator : IIncrementalGenerator
{
    private const string GenerateSolutionModelAttributeFull = "DecSm.Atom.Build.Definition.GenerateSolutionModelAttribute";
    private const string DefaultBuildDefinitionAttributeFull = "DecSm.Atom.Build.Definition.DefaultBuildDefinitionAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Create the provider for the Solution Path (from MSBuild)
        var solutionPathProvider = context.AnalyzerConfigOptionsProvider.Select((options, _) =>
            options.GlobalOptions.TryGetValue("build_property.SolutionPath", out var path) &&
            !string.IsNullOrWhiteSpace(path) &&
            path != "*Undefined*"
                ? Path.GetFullPath(path)
                : null);

        // 2. Your existing provider for Classes with the specific Attribute
        var classesProvider = context
            .SyntaxProvider
            .CreateSyntaxProvider((s, _) => s is ClassDeclarationSyntax, (ctx, _) => GetClassDeclaration(ctx))
            .WithTrackingName("GenerateSolutionModelSourceGenerator")
            .Where(t => t.attributeFound)
            .Select((t, _) => t.Item1)
            .Collect();

        // 3. Combine everything: ((Compilation, Classes), SolutionPath)
        var sourcePipeline = context
            .CompilationProvider
            .Combine(classesProvider)
            .Combine(solutionPathProvider);

        context.RegisterSourceOutput(sourcePipeline,
            (spc, source) =>
            {
                var ((compilation, classes), solutionPath) = source;
                GenerateCode(spc, compilation, classes, solutionPath);
            });
    }

    private static (ClassDeclarationSyntax, bool attributeFound) GetClassDeclaration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        foreach (var attributeSyntax in attributeListSyntax.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax)
                    .Symbol is not IMethodSymbol attributeSymbol)
                continue;

            var attributeName = attributeSymbol.ContainingType.ToDisplayString();

            if (attributeName is GenerateSolutionModelAttributeFull or DefaultBuildDefinitionAttributeFull)
                return (classDeclarationSyntax, true);
        }

        return (classDeclarationSyntax, false);
    }

    private static void GenerateCode(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classDeclarations,
        string? solutionPath)
    {
        // Safety check: if MSBuild didn't pass the path, we can't generate the specific solution model.
        if (string.IsNullOrEmpty(solutionPath))

            // Optional: Report a diagnostic here warning that $(SolutionPath) is missing.
            // This happens if building a .csproj directly without a solution context
            // or if the .targets file is missing.
            return;

        foreach (var classDeclarationSyntax in classDeclarations)
            if (compilation
                    .GetSemanticModel(classDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol classSymbol)
                GeneratePartial(context, classSymbol, classDeclarationSyntax, solutionPath!);
    }

    private static void GeneratePartial(
        SourceProductionContext context,
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax classDeclarationSyntax,
        string solutionPath)
    {
        var @namespace = classSymbol.ContainingNamespace.ToDisplayString();

        var namespaceLine = @namespace is "<global namespace>"
            ? string.Empty
            : $"namespace {@namespace};";

        var @class = classDeclarationSyntax.Identifier.Text;

        var projects = Parser.GetProjectsFromSolution(solutionPath);

        var projectFileTypeLines = string.Join("\n\n",
            projects.Select(kvp => $$"""
                                         /// <summary>
                                         ///    {{kvp.Value.Replace("\\", "/")}}
                                         /// </summary>
                                         public interface {{kvp.Key.Replace('.', '_')}} : IFileMarker
                                         {
                                             public const string Name = @"{{kvp.Key}}";

                                             static RootedPath IFileMarker.Path(IAtomFileSystem fileSystem) =>
                                                 fileSystem.CreateRootedPath(@"{{kvp.Value.Replace("\\", "/")}}");

                                             new static RootedPath Path(IAtomFileSystem fileSystem) =>
                                                 fileSystem.CreateRootedPath(@"{{kvp.Value.Replace("\\", "/")}}");
                                         }
                                     """));

        var code = $$"""
                     // <auto-generated/>

                     #nullable enable

                     using DecSm.Atom.Paths;

                     {{namespaceLine}}

                     /// <summary>
                     ///    {{solutionPath.Replace("\\", "/")}}
                     /// </summary>
                     public interface Solution : IFileMarker
                     {
                         public const string Name = @"{{Path.GetFileNameWithoutExtension(solutionPath.Replace("\\", "/"))}}";

                         static RootedPath IFileMarker.Path(IAtomFileSystem fileSystem) =>
                            fileSystem.CreateRootedPath(@"{{solutionPath.Replace("\\", "/")}}");
                     }

                     public static class Projects
                     {
                     {{projectFileTypeLines}}
                     }
                     """;

        context.AddSource($"{@class}.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}
