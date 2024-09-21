namespace DecSm.Atom.Tests.Build.Definition;

[TestFixture]
public class BuildDefinitionTests
{
    [Test]
    public void BuildDefinition_GetParam_CallsParamService()
    {
        // Arrange
        var parameterExpression = (Expression<Func<string?>>)(() => "test");

        // var paramService = new Mock<IParamService>();
        var paramService = A.Fake<IParamService>();

        // paramService
        //     .Setup(x => x.GetParam(It.Is<Expression<Func<string?>>>(s => s == parameterExpression),
        //         It.IsAny<string?>(),
        //         It.IsAny<Func<string?, string?>?>()))
        //     .Verifiable(Times.Once);

        A
            .CallTo(() => paramService.GetParam(A<Expression<Func<string?>>>.That.IsSameAs(parameterExpression),
                A<string?>._,
                A<Func<string?, string?>>._))
            .Returns("test");

        // var services = new ServiceCollection()
        //     .AddSingleton(paramService.Object)
        //     .BuildServiceProvider();

        var services = new ServiceCollection()
            .AddSingleton(paramService)
            .BuildServiceProvider();

        // var buildDefinition = new Mock<BuildDefinition>(services)
        // {
        //     CallBase = true,
        // }.Object;

        var buildDefinition = A.Fake<BuildDefinition>(x => x.WithArgumentsForConstructor(new object[] { services }));

        // Act
        buildDefinition.GetParam(parameterExpression);

        // Assert
        // paramService.Verify();

        A
            .CallTo(() => paramService.GetParam(A<Expression<Func<string?>>>._, A<string?>._, A<Func<string?, string?>>._))
            .MustHaveHappened();
    }

    [Test]
    public void BuildDefinition_WriteVariable_CallsWorkflowVariableService()
    {
        // Arrange
        const string name = "name";
        const string value = "value";

        // var workflowVariableService = new Mock<IWorkflowVariableService>();
        var workflowVariableService = A.Fake<IWorkflowVariableService>();

        // workflowVariableService
        //     .Setup(x => x.WriteVariable(name, value))
        //     .Verifiable(Times.Once);

        A
            .CallTo(() => workflowVariableService.WriteVariable(name, value))
            .Returns(Task.CompletedTask);

        // var services = new ServiceCollection()
        //     .AddSingleton(workflowVariableService.Object)
        //     .BuildServiceProvider();

        var services = new ServiceCollection()
            .AddSingleton(workflowVariableService)
            .BuildServiceProvider();

        // var buildDefinition = new Mock<BuildDefinition>(services)
        // {
        //     CallBase = true,
        // }.Object;

        var buildDefinition = A.Fake<BuildDefinition>(x => x.WithArgumentsForConstructor(new object[] { services }));

        // Act
        buildDefinition.WriteVariable(name, value);

        // Assert
        // workflowVariableService.Verify();

        A
            .CallTo(() => workflowVariableService.WriteVariable(name, value))
            .MustHaveHappened();
    }
}
