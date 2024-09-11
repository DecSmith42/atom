using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using Xunit;

namespace DecSm.Atom.SourceGenerators.Tests;

public class BuildDefinitionSourceGeneratorTests
{
    private const string InputText = "//TODO";

    private const string ExpectedText = "//TODO";

    [Fact]
    public void GenerateReportMethod()
    {
        // Create an instance of the source generator.
        var generator = new BuildDefinitionSourceGenerator();

        // Source generators should be tested using 'GeneratorDriver'.
        var driver = CSharpGeneratorDriver.Create(generator);

        // We need to create a compilation with the required source code.
        var compilation = CSharpCompilation.Create(nameof(BuildDefinitionSourceGeneratorTests),
            [CSharpSyntaxTree.ParseText(InputText)],
            [
                // To support 'System.Attribute' inheritance, add reference to 'System.Private.CoreLib'.
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            ]);

        // Run generators and retrieve all results.
        var runResult = driver
            .RunGenerators(compilation)
            .GetRunResult();

        // All generated files can be found in 'RunResults.GeneratedTrees'.
        var generatedFileSyntax = runResult.GeneratedTrees.SingleOrDefault(t => t.FilePath.EndsWith("//TODO"));

        // Compare with expected text.
        // generatedFileSyntax.ShouldSatisfyAllConditions(() => generatedFileSyntax.ShouldNotBeNull(),
        //     () => generatedFileSyntax!
        //         .GetText()
        //         .ToString()
        //         .ShouldBe(ExpectedText, StringCompareShould.IgnoreLineEndings));
        generatedFileSyntax.ShouldBeNull();
    }
}
