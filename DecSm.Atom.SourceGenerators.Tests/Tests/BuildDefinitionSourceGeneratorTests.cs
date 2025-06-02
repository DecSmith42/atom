namespace DecSm.Atom.SourceGenerators.Tests.Tests;

[TestFixture]
public class BuildDefinitionSourceGeneratorTests
{
    [Test]
    public async Task MinimalDefinition_GeneratesSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;

                              namespace TestNamespace;

                              [BuildDefinition]
                              public partial class MinimalTestDefinition : BuildDefinition;
                              """;

        // Act
        var generatedText = TestUtils.GetGeneratedSource<BuildDefinitionSourceGenerator>(source, typeof(BuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
    }

    [Test]
    public async Task DefaultDefinition_GeneratesSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;

                              namespace TestNamespace;

                              [BuildDefinition]
                              public partial class DefaultTestDefinition : DefaultBuildDefinition;
                              """;

        // Act
        var generatedText = TestUtils.GetGeneratedSource<BuildDefinitionSourceGenerator>(source, typeof(BuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
    }

    [Test]
    public async Task DefinitionWithTargetSetup_GeneratesSource()
    {
        // Arrange
        const string source = """
                              using DecSm.Atom.Build.Definition;

                              namespace TestNamespace;

                              [BuildDefinition]
                              public partial class TestDefinitionWithSetup : BuildDefinition, ITestTargetDefinition;

                              [TargetDefinition]
                              [TargetSetup]
                              public interface ITestTargetDefinition
                              {
                                  public void ConfigureBuilder(IHostApplicationBuilder builder);
                              }
                              """;

        // Act
        var generatedText = TestUtils.GetGeneratedSource<BuildDefinitionSourceGenerator>(source, typeof(BuildDefinition).Assembly);

        // Assert
        await Verify(generatedText);
    }
}
