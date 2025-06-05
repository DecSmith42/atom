using System.Reflection;
using DecSm.Atom.Build.Definition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using CodeFixVerifier = DecSm.Atom.Analyzers.Tests.ExtendedCodeFixVerifier<
    DecSm.Atom.Analyzers.AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzer,
    DecSm.Atom.Analyzers.AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamCodeFixProvider>;

namespace DecSm.Atom.Analyzers.Tests;

// ReSharper disable once InconsistentNaming
public class AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamCodeFixProviderTests
{
    private void Configure(
        CSharpCodeFixTest<AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzer,
            AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamCodeFixProvider, DefaultVerifier> configuration)
    {
        configuration.ReferenceAssemblies = ReferenceAssemblies.Net.Net90;

        configuration.TestState.AdditionalReferences.AddRange([
            MetadataReference.CreateFromFile(typeof(BuildDefinition).GetTypeInfo()
                .Assembly.Location),
        ]);
    }

    [Fact]
    public async Task RequiresParamWithDirectParam_ReplacesWithNameof()
    {
        const string sourceCode = """
                                  using DecSm.Atom.Build;
                                  using DecSm.Atom.Build.Definition;
                                  using DecSm.Atom.Params;

                                  public interface IMyTarget : IBuildAccessor
                                  {
                                      [ParamDefinition("my-param", "My Param")]
                                      string MyParam => GetParam(() => MyParam)!;

                                      Target MyTarget => t => t.RequiresParam(MyParam);
                                  }
                                  """;

        const string fixedCode = """
                                 using DecSm.Atom.Build;
                                 using DecSm.Atom.Build.Definition;
                                 using DecSm.Atom.Params;

                                 public interface IMyTarget : IBuildAccessor
                                 {
                                     [ParamDefinition("my-param", "My Param")]
                                     string MyParam => GetParam(() => MyParam)!;

                                     Target MyTarget => t => t.RequiresParam(nameof(MyParam));
                                 }
                                 """;

        var expected = CodeFixVerifier
            .Diagnostic(AT0001_TargetRequiringParamShouldNotDirectlyReferenceParamAnalyzer.DiagnosticId)
            .WithSpan(10, 45, 10, 52)
            .WithArguments("MyParam");

        await CodeFixVerifier.VerifyCodeFixAsync(sourceCode, [expected], fixedCode, Configure);
    }
}
