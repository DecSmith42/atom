namespace DecSm.Atom.SourceGenerators.Tests.Tests;

[TestFixture]
public class TargetDefinitionSourceGeneratorTests
{
    [Test]
    public async Task EmptyDefinition_GeneratesDefaultSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;

                              namespace TestNamespace;

                              [TargetDefinition]
                              public partial interface ITestTargetDefinition;
                              """;

        // Act
        var generatedText = TestUtils.GetGeneratedSource<TargetDefinitionSourceGenerator>(source, typeof(BuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
    }
}
