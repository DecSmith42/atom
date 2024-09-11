namespace DecSm.Atom.Tests.Build.Definition;

[TestFixture]
public class BuildDefinitionTests
{
    [Test]
    public void BuildDefinition_GetParam_CallsParamService()
    {
        // Arrange
        var parameterExpression = (Expression<Func<string?>>)(() => "test");

        var paramService = new Mock<IParamService>();

        paramService
            .Setup(x => x.GetParam(It.Is<Expression<Func<string?>>>(s => s == parameterExpression),
                It.IsAny<string?>(),
                It.IsAny<Func<string?, string?>?>()))
            .Verifiable(Times.Once);

        var services = new ServiceCollection()
            .AddSingleton(paramService.Object)
            .BuildServiceProvider();

        var buildDefinition = new Mock<BuildDefinition>(services)
        {
            CallBase = true,
        }.Object;

        // Act
        buildDefinition.GetParam(parameterExpression);

        // Assert
        paramService.Verify();
    }

    [Test]
    public void BuildDefinition_WriteVariable_CallsWorkflowVariableService()
    {
        // Arrange
        const string name = "name";
        const string value = "value";

        var workflowVariableService = new Mock<IWorkflowVariableService>();

        workflowVariableService
            .Setup(x => x.WriteVariable(name, value))
            .Verifiable(Times.Once);

        var services = new ServiceCollection()
            .AddSingleton(workflowVariableService.Object)
            .BuildServiceProvider();

        var buildDefinition = new Mock<BuildDefinition>(services)
        {
            CallBase = true,
        }.Object;

        // Act
        buildDefinition.WriteVariable(name, value);

        // Assert
        workflowVariableService.Verify();
    }
}
