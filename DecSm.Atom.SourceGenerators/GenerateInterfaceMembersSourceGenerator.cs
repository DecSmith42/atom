﻿using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using DeclarationResult = (Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax Declaration, bool HasAttribute);

namespace DecSm.Atom.SourceGenerators;

[Generator]
public class GenerateInterfaceMembersSourceGenerator : IIncrementalGenerator
{
    private const string GenerateInterfaceMembersAttributeFull = "DecSm.Atom.Build.Definition.GenerateInterfaceMembersAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterSourceOutput(context.CompilationProvider.Combine(context
                .SyntaxProvider
                .CreateSyntaxProvider(static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                    static (context, _) => GetClassDeclaration(context))
                .WithTrackingName("GenerateInterfaceMembersSourceGenerator")
                .Where(static declarationResult => declarationResult.HasAttribute)
                .Select(static (declarationResult, _) => declarationResult.Declaration)
                .Collect()),
            GenerateCode);

    private static DeclarationResult GetClassDeclaration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        foreach (var attributeListSyntax in classDeclarationSyntax.AttributeLists)
        foreach (var attributeSyntax in attributeListSyntax.Attributes)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(attributeSyntax);

            if (symbolInfo.Symbol is not IMethodSymbol attributeSymbol)
                continue;

            var attributeName = attributeSymbol.ContainingType.ToDisplayString();

            if (attributeName == GenerateInterfaceMembersAttributeFull)
                return (classDeclarationSyntax, true);
        }

        return (classDeclarationSyntax, false);
    }

    private static void GenerateCode(
        SourceProductionContext context,
        (Compilation Compilation, ImmutableArray<ClassDeclarationSyntax> ClassDeclarations) compilationWithClassDeclarations)
    {
        foreach (var classDeclarationSyntax in compilationWithClassDeclarations.ClassDeclarations)
            if (compilationWithClassDeclarations
                    .Compilation
                    .GetSemanticModel(classDeclarationSyntax.SyntaxTree)
                    .GetDeclaredSymbol(classDeclarationSyntax) is INamedTypeSymbol classSymbol)
                GeneratePartial(context, classSymbol, classDeclarationSyntax);
    }

    private static void GeneratePartial(
        SourceProductionContext context,
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax classDeclarationSyntax)
    {
        var @namespace = classSymbol.ContainingNamespace.ToDisplayString();

        var namespaceLine = @namespace is "<global namespace>"
            ? string.Empty
            : $"namespace {@namespace};";

        var @class = classDeclarationSyntax.Identifier.Text;
        var classFull = $"{classSymbol.ContainingNamespace}.{@class}";

        var globalUsingStaticLine = @namespace is "<global namespace>"
            ? string.Empty
            : $"global using static {classFull};";

        var interfacesWithProperties = classSymbol
            .AllInterfaces
            .SelectMany(static interfaceSymbol => interfaceSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(propertySymbol => new TypeWithProperty(interfaceSymbol, propertySymbol)))
            .Concat(classSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(propertySymbol => new TypeWithProperty(classSymbol, propertySymbol)))
            .Where(x => x.Property.DeclaredAccessibility is not Accessibility.Private &&
                        x.Property is
                        {
                            IsStatic: false,
                            Name: not "GlobalWorkflowOptions"
                            and not "Workflows"
                            and not "ParamDefinitions"
                            and not "TargetDefinitions"
                            and not "Logger"
                            and not "FileSystem"
                            and not "ProcessRunner"
                            and not "Services",
                        })
            .ToArray();

        var interfacesWithMethods = classSymbol
            .AllInterfaces
            .SelectMany(static interfaceSymbol => interfaceSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Select(methodSymbol => new TypeWithMethod(interfaceSymbol, methodSymbol)))
            .Concat(classSymbol
                .GetMembers()
                .OfType<IMethodSymbol>()
                .Select(methodSymbol => new TypeWithMethod(classSymbol, methodSymbol)))
            .Where(x => x.Method.DeclaredAccessibility is not Accessibility.Private &&
                        !x.Method.IsStatic &&
                        !x.Method.Name.StartsWith("get_") &&
                        x.Method.Name is not ".ctor"
                            and not "ConfigureBuilder"
                            and not "GetService"
                            and not "GetServices"
                            and not "GetParam")
            .ToArray();

        // We want to generate methods in the partial class that allow us to directly access properties and methods
        // that have default implementations in parent interfaces.
        // E.g.
        //
        // (in IInterface)
        // string Name => "Bob";
        // string GetName() => Name;
        //
        // (generated in PartialClass : IInterface)
        // // ReSharper disable once MemberHidesInterfaceMemberWithDefaultImplementation
        // private string Name => ((IInterface)this).Name;
        //
        // // ReSharper disable once MemberHidesInterfaceMemberWithDefaultImplementation
        // private string GetName() => ((IInterface)this).GetName();

        var propertyLines = interfacesWithProperties
            .Select(typeWithProperty =>
            {
                var interfaceName = typeWithProperty.Interface.ToDisplayString();
                var propertyName = typeWithProperty.Property.Name;
                var propertyType = typeWithProperty.Property.Type.ToDisplayString();

                return $"private {propertyType} {propertyName} => (({interfaceName})this).{propertyName};";
            })
            .ToArray();

        var methodLines = interfacesWithMethods
            .Select(typeWithMethod =>
            {
                var interfaceName = typeWithMethod.Interface.ToDisplayString();
                var methodName = typeWithMethod.Method.Name;

                var methodParameters = string.Join(", ",
                    typeWithMethod.Method.Parameters.Select(param => $"{param.Type.ToDisplayString()} {param.Name}"));

                var methodReturnType = typeWithMethod.Method.ReturnType.ToDisplayString();

                return typeWithMethod.Method.IsGenericMethod
                    ? $"private {methodReturnType} {methodName}<{string.Join(", ", typeWithMethod.Method.TypeParameters.Select(param => param.Name))}>({methodParameters}) => (({interfaceName})this).{methodName}<{string.Join(", ", typeWithMethod.Method.TypeParameters.Select(param => param.Name))}>({string.Join(", ", typeWithMethod.Method.Parameters.Select(param => param.Name))});"
                    : $"private {methodReturnType} {methodName}({methodParameters}) => (({interfaceName})this).{methodName}({string.Join(", ", typeWithMethod.Method.Parameters.Select(param => param.Name))});";
            })
            .ToArray();

        var propertyCode = propertyLines.Any()
            ? $"{string.Join("\n\n", propertyLines)}"
            : string.Empty;

        var methodCode = methodLines.Any()
            ? $"{string.Join("\n\n", methodLines)}"
            : string.Empty;

        var code = $$"""
                     // <auto-generated/>

                     // ReSharper disable MemberHidesInterfaceMemberWithDefaultImplementation

                     #nullable enable

                     {{globalUsingStaticLine}}

                     {{namespaceLine}}

                     partial class {{@class}}
                     {
                     {{propertyCode}}

                     {{methodCode}}
                     }

                     """;

        context.AddSource($"{@class}.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private record struct TypeWithProperty(INamedTypeSymbol Interface, IPropertySymbol Property);

    private record struct TypeWithMethod(INamedTypeSymbol Interface, IMethodSymbol Method);
}
