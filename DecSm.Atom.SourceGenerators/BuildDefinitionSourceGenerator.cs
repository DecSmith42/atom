using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DecSm.Atom.SourceGenerators;

[Generator]
public class BuildDefinitionSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Filter classes annotated with the BuildDefinition attribute. Only filtered Syntax Nodes can trigger code generation.
        var provider = context
            .SyntaxProvider
            .CreateSyntaxProvider((s, _) => s is ClassDeclarationSyntax, (ctx, _) => GetClassDeclarationForSourceGen(ctx))
            .Where(t => t.reportAttributeFound)
            .Select((t, _) => t.Item1);

        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
            (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
    }

    private static (ClassDeclarationSyntax, bool reportAttributeFound) GetClassDeclarationForSourceGen(GeneratorSyntaxContext context)
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
            if (attributeName == "DecSm.Atom.Build.Definition.BuildDefinitionAttribute")
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

            var namespaceLine = classSymbol.ContainingNamespace.ToDisplayString() is "<global namespace>"
                ? string.Empty
                : $"namespace {classSymbol.ContainingNamespace.ToDisplayString()};";

            var className = classDeclarationSyntax.Identifier.Text;

            GeneratePartialBuild(context, classSymbol, namespaceLine, className);
        }
    }

    private static void GeneratePartialBuild(
        SourceProductionContext context,
        INamedTypeSymbol classSymbol,
        string namespaceLine,
        string className)
    {
        // Get all defined targets (property that returns Target) in all inherited interfaces
        var interfaceTargets = classSymbol
            .AllInterfaces
            .SelectMany(i => i
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(x => (Interface: i, Property: x)))
            .Where(p => p.Property.Type.Name == "Target")
            .Select(p => (p.Interface, p.Property))
            .ToList();

        // Get all defined Params (Property with ParamDefinitionAttribute) in all inherited interfaces,
        // along with the ParamDefinitionAttribute.Name value
        var interfaceParams = classSymbol
            .AllInterfaces
            .SelectMany(i => i
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(x => (Interface: i, Property: x)))
            .Where(p => p
                .Property
                .GetAttributes()
                .Any(a => a.AttributeClass?.Name == "ParamDefinitionAttribute"))
            .Select(p => (p.Interface, p.Property, Attribute: p
                .Property
                .GetAttributes()
                .First(a => a.AttributeClass?.Name == "ParamDefinitionAttribute")))
            .ToList();

        // Get all defined Secrets (Property with SecretDefinitionAttribute) in all inherited interfaces,
        // along with the SecretDefinitionAttribute.Name value
        var interfaceSecretParams = classSymbol
            .AllInterfaces
            .SelectMany(i => i
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(x => (Interface: i, Property: x)))
            .Where(p => p
                .Property
                .GetAttributes()
                .Any(a => a.AttributeClass?.Name == "SecretDefinitionAttribute"))
            .Select(p => (p.Interface, p.Property, Attribute: p
                .Property
                .GetAttributes()
                .First(a => a.AttributeClass?.Name == "SecretDefinitionAttribute")))
            .ToList();

        // Generate a static accessor for each target
        var targetsPropertiesBodies = interfaceTargets.Select(p =>
            $"        public static string {p.Property.Name} = nameof({p.Interface}.{p.Property.Name});");

        // Generate a static accessor for each CommandDefinition
        var commandDefsPropertyBodies = interfaceTargets.Select(p =>
            $"        public static DecSm.Atom.Workflows.Definition.Command.CommandDefinition {p.Property.Name} = new(nameof({p.Interface}.{p.Property.Name}));");

        // Generate the Targets property
        var targetDefinitionsPropertyBody = interfaceTargets.Select(p =>
            $$"""        { "{{p.Property.Name}}", (({{p.Interface}})this).{{p.Property.Name}} },""");

        // Generate a static accessor for each param
        var paramsPropertiesBodies = interfaceParams.Select(p =>
            $"        public static string {p.Property.Name} = nameof({p.Interface}.{p.Property.Name});");

        // Generate a static accessor for each secret param
        var secretParamsPropertiesBodies = interfaceSecretParams.Select(p =>
            $"        public static string {p.Property.Name} = nameof({p.Interface}.{p.Property.Name});");

        // Generate the ParamDefinitions property
        var paramDefinitionsPropertyBody = interfaceParams
            .Select(x =>
            {
                var interfaceName = $"{x.Interface.ContainingNamespace.ToDisplayString()}.{x.Interface.Name}";

                return new
                {
                    x.Property.Name,
                    Interface = $"typeof({interfaceName})",
                    AttrubuteName = x.Attribute.AttributeClass!.ToDisplayString(),
                    AttributeArgs = x
                        .Attribute
                        .ConstructorArguments
                        .Select(a => a.Value)
                        .Select(a => a is string s
                            ? $"\"{s}\""
                            : a)
                        .Select(a => a ?? "null"),
                };
            })
            .Select(p =>
                $$"""        { "{{p.Name}}", new("{{p.Name}}", new {{p.AttrubuteName}}({{string.Join(", ", p.AttributeArgs)}})) },""");

        // Generate the SecretParamDefinitions property
        var secretParamDefinitionsPropertyBody = interfaceSecretParams
            .Select(x =>
            {
                var interfaceName = $"{x.Interface.ContainingNamespace.ToDisplayString()}.{x.Interface.Name}";

                return new
                {
                    x.Property.Name,
                    Interface = $"typeof({interfaceName})",
                    AttrubuteName = x.Attribute.AttributeClass!.ToDisplayString(),
                    AttributeArgs = x
                        .Attribute
                        .ConstructorArguments
                        .Select(a => a.Value)
                        .Select(a => a is string s
                            ? $"\"{s}\""
                            : a)
                        .Select(a => a ?? "null"),
                };
            })
            .Select(p =>
                $$"""        { "{{p.Name}}", new("{{p.Name}}", new {{p.AttrubuteName}}({{string.Join(", ", p.AttributeArgs)}})) },""");

        // Generate the ParamAccessors property
        var paramAccessorsPropertyBody = interfaceParams
            .Select(x =>
            {
                var interfaceName = $"{x.Interface.ContainingNamespace.ToDisplayString()}.{x.Interface.Name}";

                return new
                {
                    x.Property.Name,
                    Interface = $"typeof({interfaceName})",
                    Accessor = $"() => (({interfaceName})this).{x.Property.Name}",
                };
            })
            .Select(p => $$"""        { "{{p.Name}}", {{p.Accessor}} },""");

        // Build up the source code
        var code = $$"""
                     // <auto-generated/>

                     #nullable enable

                     using System;
                     using System.Collections.Generic;
                     using System.Reflection;
                     using DecSm.Atom;
                     using DecSm.Atom.Build.Definition;
                     using DecSm.Atom.Params;
                     using JetBrains.Annotations;

                     {{namespaceLine}}

                     [PublicAPI]
                     partial class {{className}}
                     {
                         public {{className}}(IServiceProvider services) : base(services) { }
                         
                         private Dictionary<string, Target>? _targetDefinitions;
                         private Dictionary<string, ParamDefinition>? _paramDefinitions;
                         private Dictionary<string, Func<string?>>? _paramAccessors;
                     
                         public override Dictionary<string, Target> TargetDefinitions => _targetDefinitions ??= new Dictionary<string, Target>
                         {
                     {{string.Join("\n", targetDefinitionsPropertyBody)}}
                         };
                         
                         public override Dictionary<string, ParamDefinition> ParamDefinitions => _paramDefinitions ??= new Dictionary<string, ParamDefinition>
                         {
                     {{string.Join("\n", paramDefinitionsPropertyBody)}}
                     {{string.Join("\n", secretParamDefinitionsPropertyBody)}}
                         };
                         
                         public override Dictionary<string, Func<string?>> ParamAccessors => _paramAccessors ??= new Dictionary<string, Func<string?>>
                         {
                     {{string.Join("\n", paramAccessorsPropertyBody)}}
                         };
                     
                         public static class Targets
                         {
                     {{string.Join("\n\n", targetsPropertiesBodies)}}
                         }
                         
                         public static class Commands
                         {
                     {{string.Join("\n\n", commandDefsPropertyBodies)}}
                         }
                         
                         public static class Params
                         {
                     {{string.Join("\n\n", paramsPropertiesBodies)}}
                         }
                         
                         public static class Secrets
                         {
                     {{string.Join("\n\n", secretParamsPropertiesBodies)}}
                         }
                     }

                     """;

        // Add the source code to the compilation.
        context.AddSource($"{className}.g.cs", SourceText.From(code, Encoding.UTF8));
    }
}