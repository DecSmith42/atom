using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using DeclarationResult = (Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax Declaration, bool HasAttribute);

namespace DecSm.Atom.SourceGenerators;

[Generator]
public class BuildDefinitionSourceGenerator : IIncrementalGenerator
{
    private const string BuildDefinitionAttributeFull = "DecSm.Atom.Build.Definition.BuildDefinitionAttribute";
    private const string Target = "Target";
    private const string TargetFull = "DecSm.Atom.Build.Definition.Target";
    private const string ConfigureHostBuilderAttributeFull = "DecSm.Atom.Hosting.ConfigureHostBuilderAttribute";
    private const string ConfigureHostAttributeFull = "DecSm.Atom.Hosting.ConfigureHostAttribute";
    private const string WorkflowTargetDefinitionFull = "DecSm.Atom.Workflows.Definition.WorkflowTargetDefinition";
    private const string ParamDefinitionFull = "DecSm.Atom.Params.ParamDefinition";
    private const string ParamDefinitionAttribute = "ParamDefinitionAttribute";
    private const string SecretDefinitionAttribute = "SecretDefinitionAttribute";
    private const string IBuildDefinition = "IBuildDefinition";
    private const string IBuildDefinitionFull = "DecSm.Atom.Build.Definition.IBuildDefinition";
    private const string Register = "Register";
    private const string RegisterTarget = "RegisterTarget";

    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        context.RegisterSourceOutput(context.CompilationProvider.Combine(context
                .SyntaxProvider
                .CreateSyntaxProvider(static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
                    static (context, _) => GetClassDeclaration(context))
                .WithTrackingName("BuildDefinitionSourceGenerator")
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

            if (attributeName == BuildDefinitionAttributeFull)
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

    private static string SimpleName(string fullName) =>
        fullName
            .Split('.')
            .Last();

    private static List<InterfaceWithProperty> DeduplicateTargets(IEnumerable<InterfaceWithProperty> interfacesWithTargets)
    {
        var interfacesWithTargetsList = interfacesWithTargets.ToList();

        while (true)
        {
            var continueWhile = false;

            foreach (var interfaceWithTarget in interfacesWithTargetsList)
            {
                var (interfaceSymbol, propertySymbol) = interfaceWithTarget;

                var duplicateInterfaceTarget = new InterfaceWithProperty();

                var matches = interfacesWithTargetsList.Where(p =>
                    SimpleName(p.Property.Name) == SimpleName(propertySymbol.Name) &&
                    !p.Interface.Equals(interfaceSymbol, SymbolEqualityComparer.IncludeNullability));

                foreach (var match in matches)
                {
                    duplicateInterfaceTarget = match;

                    break;
                }

                if (duplicateInterfaceTarget == default)
                    continue;

                var interfaceTargetInheritsFromDuplicate =
                    interfaceWithTarget.Interface.AllInterfaces.Contains(duplicateInterfaceTarget.Interface);

                interfacesWithTargetsList.Remove(interfaceTargetInheritsFromDuplicate
                    ? duplicateInterfaceTarget
                    : interfaceWithTarget);

                continueWhile = true;

                break;
            }

            if (!continueWhile)
                break;
        }

        return interfacesWithTargetsList;
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
        var classFull = $"{@namespace}.{@class}";

        var interfacesWithProperties = classSymbol
            .AllInterfaces
            .SelectMany(static interfaceSymbol => interfaceSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Select(propertySymbol => new InterfaceWithProperty(interfaceSymbol, propertySymbol)))
            .ToArray();

        var interfacesWithTargets = DeduplicateTargets(interfacesWithProperties.Where(static p => p.Property.Type.Name is Target));

        var interfacesWithParams = interfacesWithProperties
            .Where(static p => p
                .Property
                .GetAttributes()
                .Any(static attribute => attribute.AttributeClass?.Name is ParamDefinitionAttribute or SecretDefinitionAttribute))
            .Select(static interfaceWithProperty => new PropertyWithAttribute(interfaceWithProperty.Property,
                interfaceWithProperty
                    .Property
                    .GetAttributes()
                    .First(attribute => attribute.AttributeClass?.Name is ParamDefinitionAttribute or SecretDefinitionAttribute)))
            .ToList();

        var interfacesWithConfigureBuilder = classSymbol
            .AllInterfaces
            .Where(static @interface => @interface
                .GetAttributes()
                .FirstOrDefault(static attribute =>
                    attribute.AttributeClass?.ToDisplayString() is ConfigureHostBuilderAttributeFull) is not null)
            .ToArray();

        var interfacesWithConfigureHost = classSymbol
            .AllInterfaces
            .Where(static @interface => @interface
                .GetAttributes()
                .FirstOrDefault(static attribute => attribute.AttributeClass?.ToDisplayString() is ConfigureHostAttributeFull) is not null)
            .ToArray();

        var targetDefinitionsPropertyBodyLines = interfacesWithTargets
            .Select(static p =>
                $$"""        { "{{SimpleName(p.Property.Name)}}", (({{p.Interface}})this).{{SimpleName(p.Property.Name)}} },""")
            .ToArray();

        var targetDefinitionsField = targetDefinitionsPropertyBodyLines.Any()
            ? $"    private System.Collections.Generic.IReadOnlyDictionary<string, {TargetFull}>? _targetDefinitions;"
            : "    // Build has no defined targets";

        var targetDefinitionsProperty = targetDefinitionsPropertyBodyLines.Any()
            ? $$"""
                    public override System.Collections.Generic.IReadOnlyDictionary<string, {{TargetFull}}> TargetDefinitions => _targetDefinitions ??= new System.Collections.Generic.Dictionary<string, {{TargetFull}}>
                    {
                {{string.Join("\n", targetDefinitionsPropertyBodyLines)}}
                    };
                """
            : $$"""    public override System.Collections.Generic.IReadOnlyDictionary<string, {{TargetFull}}> TargetDefinitions { get; } = new System.Collections.Generic.Dictionary<string, {{TargetFull}}>();""";

        var paramDefinitionsPropertyBodyLines = interfacesWithParams
            .Select(static x => new
            {
                x.Property.Name,
                ArgName = (string)x.Attribute.ConstructorArguments[0].Value!,
                Description = (string)x
                    .Attribute
                    .ConstructorArguments
                    .Skip(1)
                    .First()
                    .Value!,
                DefaultValue = x
                    .Attribute
                    .ConstructorArguments
                    .Skip(2)
                    .First()
                    .Value is string s
                    ? $"\"{s}\""
                    : "null",
                Sources = $"({x
                    .Attribute
                    .ConstructorArguments
                    .Skip(3)
                    .First().Type!.ToDisplayString()}){x
                    .Attribute
                    .ConstructorArguments
                    .Skip(3)
                    .First().Value}",
                IsSecret = x.Attribute.AttributeClass?.Name is SecretDefinitionAttribute,
            })
            .Select(static p => $$"""
                                          {
                                              "{{p.Name}}", new("{{p.Name}}")
                                              {
                                                  ArgName = "{{p.ArgName}}",
                                                  Description = "{{p.Description}}",
                                                  DefaultValue = {{p.DefaultValue}},
                                                  Sources = {{p.Sources}},
                                                  IsSecret = {{p.IsSecret.ToString().ToLower()}},
                                              }
                                          },
                                  """)
            .ToArray();

        var paramDefinitionsProperty = paramDefinitionsPropertyBodyLines.Any()
            ? $$"""
                    public override System.Collections.Generic.IReadOnlyDictionary<string, {{ParamDefinitionFull}}> ParamDefinitions { get; } = new System.Collections.Generic.Dictionary<string, {{ParamDefinitionFull}}>
                    {
                {{string.Join("\n", paramDefinitionsPropertyBodyLines)}}
                    };
                """
            : $$"""    public override System.Collections.Generic.IReadOnlyDictionary<string, {{ParamDefinitionFull}}> ParamDefinitions { get; } = new System.Collections.Generic.Dictionary<string, {{ParamDefinitionFull}}>();""";


        var targetsPropertiesLines = interfacesWithTargets
            .Select(static p =>
                $"""        public static {WorkflowTargetDefinitionFull} {SimpleName(p.Property.Name)} = new("{SimpleName(p.Property.Name)}");""")
            .ToArray();

        var targetsClass = targetsPropertiesLines.Any()
            ? $$"""
                    [JetBrains.Annotations.PublicAPI]
                    private static class Targets
                    {
                {{string.Join("\n", targetsPropertiesLines)}}
                    }

                    private static {{WorkflowTargetDefinitionFull}} Target(string name) => name switch
                    {
                {{string.Join("\n", interfacesWithTargets.Select(static p => $"""        "{SimpleName(p.Property.Name)}" => Targets.{SimpleName(p.Property.Name)},"""))}}
                        _ => throw new System.ArgumentException($"Target with name '{name}' is not defined in the build definition.", nameof(name)),
                    };
                """
            : "    // Build has no defined targets";

        var paramsPropertiesLines = interfacesWithParams
            .Select(static p => $"""        public static string {p.Property.Name} = "{SimpleName(p.Property.Name)}";""")
            .ToArray();

        var paramsClass = paramsPropertiesLines.Any()
            ? $$"""
                    [JetBrains.Annotations.PublicAPI]
                    private static class Params
                    {
                {{string.Join("\n", paramsPropertiesLines)}}
                    }
                """
            : "    // Build has no defined params";

        var registerTargetsToServicesLines = classSymbol
            .AllInterfaces
            .Where(static x => x.AllInterfaces.Any(i => i.Name is IBuildDefinition))
            .Select(static @interface => @interface
                .GetMembers($"{IBuildDefinitionFull}.{Register}")
                .Any()
                ? $"""

                           Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddSingleton<{@interface}>(builder.Services, static p => ({@interface})p.GetRequiredService<{IBuildDefinitionFull}>());
                           {RegisterTarget}<{@interface}>(services);
                   """
                : $"        Microsoft.Extensions.DependencyInjection.Extensions.ServiceCollectionDescriptorExtensions.TryAddSingleton<{@interface}>(builder.Services, static p => ({@interface})p.GetRequiredService<{IBuildDefinitionFull}>());");

        var configureBuildHostBuilderLines = interfacesWithConfigureBuilder.Select(static @interface =>
            $"        {@interface}.ConfigureBuilder(builder);");

        var configureBuildHostBuilderMethodBodyLines = registerTargetsToServicesLines
            .Concat(configureBuildHostBuilderLines)
            .ToArray();

        var configureBuildHostMethodBodyLines = interfacesWithConfigureHost
            .Select(static @interface => $"        {@interface}.ConfigureHost(builder);")
            .ToArray();

        var configureBuildHostBuilderMethod = configureBuildHostBuilderMethodBodyLines.Any() || configureBuildHostMethodBodyLines.Any()
            ? $$"""
                    public void ConfigureBuildHostBuilder(Microsoft.Extensions.Hosting.IHostApplicationBuilder builder)
                    {
                {{string.Join("\n", configureBuildHostBuilderMethodBodyLines)}}
                    }
                """
            : "    // Build has no defined HostBuilder configuration";

        var configureBuildHostMethod = configureBuildHostBuilderMethodBodyLines.Any() || configureBuildHostMethodBodyLines.Any()
            ? $$"""
                    public void ConfigureBuildHost(Microsoft.Extensions.Hosting.IHost builder)
                    {
                {{string.Join("\n", configureBuildHostMethodBodyLines)}}
                    }
                """
            : "    // Build has no defined Host configuration";

        var configureHostInherit = configureBuildHostBuilderMethodBodyLines.Any() || configureBuildHostMethodBodyLines.Any()
            ? ", DecSm.Atom.Hosting.IConfigureHost"
            : string.Empty;

        var code = $$"""
                     // <auto-generated/>

                     #nullable enable

                     global using static {{classFull}};
                     using Microsoft.Extensions.DependencyInjection;

                     {{namespaceLine}}

                     [JetBrains.Annotations.PublicAPI]
                     partial class {{@class}} : {{IBuildDefinitionFull}}{{configureHostInherit}}
                     {
                     {{targetDefinitionsField}}

                         public {{@class}}(System.IServiceProvider services) : base(services) { }

                     {{targetDefinitionsProperty}}

                     {{paramDefinitionsProperty}}

                     {{targetsClass}}

                     {{paramsClass}}

                     {{configureBuildHostBuilderMethod}}

                     {{configureBuildHostMethod}}
                     }

                     """;

        context.AddSource($"{@class}.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private record struct InterfaceWithProperty(INamedTypeSymbol Interface, IPropertySymbol Property);

    private record struct PropertyWithAttribute(IPropertySymbol Property, AttributeData Attribute);
}
