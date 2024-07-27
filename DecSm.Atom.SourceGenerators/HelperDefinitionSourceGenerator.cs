using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DecSm.Atom.SourceGenerators;

[Generator]
public class HelperDefinitionSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter classes annotated with the [HelperDefinition] attribute. Only filtered Syntax Nodes can trigger code generation.
        var provider = context
            .SyntaxProvider
            .CreateSyntaxProvider((s, _) => s is InterfaceDeclarationSyntax, (ctx, _) => GetInterfaceDeclarationForSourceGen(ctx))
            .Where(t => t.reportAttributeFound)
            .Select((t, _) => t.Item1);

        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
    }

    private static (InterfaceDeclarationSyntax, bool reportAttributeFound) GetInterfaceDeclarationForSourceGen(
        GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;

        // Go through all attributes of the class.
        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        foreach (var attributeSyntax in attributeListSyntax.Attributes)
        {
            if (context.SemanticModel.GetSymbolInfo(attributeSyntax)
                    .Symbol is not IMethodSymbol attributeSymbol)
                continue; // if we can't get the symbol, ignore it

            var attributeName = attributeSymbol.ContainingType.ToDisplayString();

            // Check the full name of the [Report] attribute.
            if (attributeName == "DecSm.Atom.Build.Definition.HelperDefinitionAttribute")
                return (classDeclarationSyntax, true);
        }

        return (classDeclarationSyntax, false);
    }

    private void GenerateCode(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<InterfaceDeclarationSyntax> classDeclarations)
    {
        // Go through all filtered class declarations.
        foreach (var classDeclarationSyntax in classDeclarations)
        {
            // We need to get semantic model of the class to retrieve metadata.
            var semanticModel = compilation.GetSemanticModel(classDeclarationSyntax.SyntaxTree);

            // Symbols allow us to get the compile-time information.
            if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol interfaceSymbol)
                continue;

            var namespaceLine = interfaceSymbol.ContainingNamespace.ToDisplayString() is "<global namespace>"
                ? string.Empty
                : $"namespace {interfaceSymbol.ContainingNamespace.ToDisplayString()};";

            var interfaceName = classDeclarationSyntax.Identifier.Text;

            // Attribute: public sealed class HelperDefinitionAttribute(params Type[] types) : Attribute;
            // We want to get the types passed to the attribute, then we can use them to generate the source code.
            var attributeTypes = interfaceSymbol
                .GetAttributes()
                .First(a => a.AttributeClass?.ToDisplayString() == "DecSm.Atom.Build.Definition.HelperDefinitionAttribute")
                .ConstructorArguments
                .First()
                .Values
                .Select(x => x.Value)
                .OfType<INamedTypeSymbol>()
                .ToArray();

            var shortcutMethodsBody = new StringBuilder();

            foreach (var attributeType in attributeTypes)
            {
                var type = attributeType.ToDisplayString();

                var methods = attributeType
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .ToArray();

                foreach (var method in methods)
                {
                    var returnType = method.ReturnType.ToDisplayString();
                    var methodName = method.Name;

                    var parameters = string.Join(", ",
                        method.Parameters.Select(p =>
                        {
                            // if the param has a default value, we need to include it in the generated code
                            if (!p.HasExplicitDefaultValue)
                                return $"{p.Type.ToDisplayString()} {p.Name}";

                            if (p.ExplicitDefaultValue is null)
                                return $"{p.Type.ToDisplayString()} {p.Name} = {(p.Type.IsValueType ? "default" : "null")}";

                            if (p.Type.SpecialType == SpecialType.System_Boolean)
                                return $"{p.Type.ToDisplayString()} {p.Name} = {p
                                    .ExplicitDefaultValue
                                    .ToString()
                                    .ToLower()}";

                            return $"{p.Type.ToDisplayString()} {p.Name} = {p.ExplicitDefaultValue}";
                        }));

                    var passedParameters = string.Join(", ", method.Parameters.Select(p => p.Name));

                    shortcutMethodsBody.AppendLine($"""
                                                        {returnType} {methodName}({parameters}) =>
                                                            Services
                                                            .GetRequiredService<{type}>()
                                                            .{methodName}({passedParameters});
                                                    """);

                    shortcutMethodsBody.AppendLine();
                }
            }

            // Build up the source code
            var code = $$"""
                         // <auto-generated/>

                         #nullable enable

                         using System;
                         using System.Collections.Generic;
                         using DecSm.Atom;
                         using DecSm.Atom.Build.Definition;
                         using JetBrains.Annotations;
                         using Microsoft.Extensions.DependencyInjection;
                         using Microsoft.Extensions.Logging;

                         {{namespaceLine}}

                         [PublicAPI]
                         partial interface {{interfaceName}} : IBuildDefinition
                         {
                         {{shortcutMethodsBody}}
                         }

                         """;

            // Add the source code to the compilation.
            context.AddSource($"{interfaceName}.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }
}