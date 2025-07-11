using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator - perf

namespace DecSm.Atom.SourceGenerators;

[Generator]
public class GenerateEntryPointSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter classes annotated with the BuildDefinition attribute. Only filtered Syntax Nodes can trigger code generation.
        var provider = context
            .SyntaxProvider
            .CreateSyntaxProvider((s, _) => s is ClassDeclarationSyntax, (ctx, _) => GetClassDeclarationForSourceGen(ctx))
            .Where(t => t.attributeFound)
            .Select((t, _) => t.Item1);

        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
    }

    private static (ClassDeclarationSyntax, bool attributeFound) GetClassDeclarationForSourceGen(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        // Go through all attributes of the class.
        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        foreach (var attributeSyntax in attributeListSyntax.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax)
                    .Symbol is not IMethodSymbol attributeSymbol)
                continue; // if we can't get the symbol, ignore it

            var attributeName = attributeSymbol.ContainingType.ToDisplayString();

            // Check the full name of the BuildDefinition attribute.
            if (attributeName == "DecSm.Atom.Hosting.GenerateEntryPointAttribute")
                return (classDeclarationSyntax, true);
        }

        return (classDeclarationSyntax, false);
    }

    private static void GenerateCode(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax> classDeclarations)
    {
        // Go through all filtered class declarations.
        foreach (var classDeclarationSyntax in classDeclarations)
        {
            // We need to get semantic model of the class to retrieve metadata.
            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            // Symbols allow us to get the compile-time information.
            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
                continue;

            var className = classDeclarationSyntax.Identifier.Text;

            GeneratePartialBuild(context, classSymbol, className);
        }
    }

    private static void GeneratePartialBuild(SourceProductionContext context, INamedTypeSymbol classSymbol, string className)
    {
        var containingNamespace = classSymbol.ContainingNamespace.ToDisplayString();

        var fullClassName = containingNamespace is "<global namespace>" or "global"
            ? className
            : $"{containingNamespace}.{className}";

        // Build up the source code
        var code = $"""
                    // <auto-generated/>

                    DecSm.Atom.Hosting.AtomHost.Run<{fullClassName}>(args);
                    """;

        context.AddSource($"{className}.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}
