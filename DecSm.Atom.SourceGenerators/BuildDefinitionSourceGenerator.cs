using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using DeclarationResult = (Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax Declaration, bool HasAttribute);

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator - perf

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
    private const string IBuildAccessor = "IBuildAccessor";
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

    private static List<TypeWithProperty> DeduplicateTargets(IEnumerable<TypeWithProperty> interfacesWithTargets)
    {
        var interfacesWithTargetsList = interfacesWithTargets.ToList();

        while (true)
        {
            var continueWhile = false;

            foreach (var interfaceWithTarget in interfacesWithTargetsList)
            {
                var (interfaceSymbol, propertySymbol) = interfaceWithTarget;

                var duplicateInterfaceTarget = new TypeWithProperty();

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
                .Any(static attribute => attribute.AttributeClass?.ToDisplayString() is ConfigureHostBuilderAttributeFull))
            .ToArray();

        var interfacesWithConfigureHost = classSymbol
            .AllInterfaces
            .Where(static @interface => @interface
                .GetAttributes()
                .Any(static attribute => attribute.AttributeClass?.ToDisplayString() is ConfigureHostAttributeFull))
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
                Sources = $"({x
                    .Attribute
                    .ConstructorArguments
                    .Skip(2)
                    .First().Type!.ToDisplayString()}){x
                    .Attribute
                    .ConstructorArguments
                    .Skip(2)
                    .First().Value}",
                IsSecret = x.Attribute.AttributeClass?.Name is SecretDefinitionAttribute,
                ChainedParams = x
                    .Attribute
                    .ConstructorArguments
                    .Skip(3)
                    .FirstOrDefault()
                    .Kind is TypedConstantKind.Array
                    ? x
                        .Attribute
                        .ConstructorArguments
                        .Skip(3)
                        .First()
                        .Values
                    : [],
            })
            .Select(static p => $$"""
                                          {
                                              "{{p.Name}}", new("{{p.Name}}")
                                              {
                                                  ArgName = "{{p.ArgName}}",
                                                  Description = "{{p.Description}}",
                                                  Sources = {{p.Sources}},
                                                  IsSecret = {{p.IsSecret.ToString().ToLower()}},
                                                  ChainedParams = {{(p.ChainedParams.IsDefaultOrEmpty ? "[]" : $"[ {string.Join(", ", p.ChainedParams.Select(v => $"\"{v.Value?.ToString() ?? string.Empty}\""))} ]")}},
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

        var accessParamMethodLines = interfacesWithParams
            .Select(p => p.Property)
            .Select(p => $"""
                                      "{SimpleName(p.Name)}" => (({p.ContainingType})this).{SimpleName(p.Name)},
                          """)
            .ToArray();

        var accessParamMethod = accessParamMethodLines.Any()
            ? $$"""
                    public override object? AccessParam(string paramName) =>
                        paramName switch
                        {
                {{string.Join("\n", accessParamMethodLines)}}
                        _ => throw new System.ArgumentException($"Param with name '{paramName}' is not defined in the build definition.", nameof(paramName)),
                        };
                """
            : """    public override object? AccessParam(string paramName) => throw new System.ArgumentException($"Param with name '{paramName}' is not defined in the build definition.", nameof(paramName));""";

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
            .Where(static x => x.AllInterfaces.Any(i => i.Name is IBuildDefinition or IBuildAccessor))
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

                     {{globalUsingStaticLine}}
                     using System.Diagnostics.CodeAnalysis;
                     using System.Linq.Expressions;
                     using Microsoft.Extensions.DependencyInjection;
                     using Microsoft.Extensions.Logging;
                     using DecSm.Atom.Build.Definition;
                     using DecSm.Atom.Params;
                     using DecSm.Atom.Paths;
                     using DecSm.Atom.Process;

                     {{namespaceLine}}

                     [JetBrains.Annotations.PublicAPI]
                     partial class {{@class}} : {{IBuildDefinitionFull}}{{configureHostInherit}}
                     {
                     {{targetDefinitionsField}}

                         public {{@class}}(System.IServiceProvider services) : base(services) { }

                         private ILogger Logger => Services.GetRequiredService<ILoggerFactory>().CreateLogger("{{classFull}}");

                         private IAtomFileSystem FileSystem => GetService<IAtomFileSystem>();

                         private IProcessRunner ProcessRunner => GetService<IProcessRunner>();

                         private T GetService<T>()
                             where T : notnull =>
                             typeof(T).GetInterface(nameof(IBuildDefinition)) != null
                                 ? (T)(IBuildDefinition)this
                                 : Services.GetRequiredService<T>();

                        private IEnumerable<T> GetServices<T>()
                             where T : notnull =>
                             typeof(T).GetInterface(nameof(IBuildDefinition)) != null
                                 ? [(T)(IBuildDefinition)this]
                                 : Services.GetServices<T>();

                         [return: NotNullIfNotNull(nameof(defaultValue))]
                         private T? GetParam<T>(Expression<Func<T?>> parameterExpression, T? defaultValue = default, Func<string?, T?>? converter = null) =>
                             Services
                                 .GetRequiredService<IParamService>()
                                 .GetParam(parameterExpression, defaultValue, converter);


                     {{targetDefinitionsProperty}}

                     {{paramDefinitionsProperty}}

                     {{accessParamMethod}}

                     {{targetsClass}}

                     {{paramsClass}}

                     {{configureBuildHostBuilderMethod}}

                     {{configureBuildHostMethod}}
                     }

                     """;

        context.AddSource($"{@class}.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private record struct TypeWithProperty(INamedTypeSymbol Interface, IPropertySymbol Property);

    private record struct PropertyWithAttribute(IPropertySymbol Property, AttributeData Attribute);
}
