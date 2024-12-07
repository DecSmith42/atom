namespace DecSm.Atom.Tests.ClassTests.Build.Definition;

[TestFixture]
public class BuildDefinitionTests
{
    [Test]
    public void GetParam_CallsParamService()
    {
        // Arrange
        var parameterExpression = (Expression<Func<string?>>)(() => "test");

        var paramService = A.Fake<IParamService>();

        A
            .CallTo(() => paramService.GetParam(A<Expression<Func<string?>>>.That.IsSameAs(parameterExpression),
                A<string?>._,
                A<Func<string?, string?>>._))
            .Returns("test");

        var services = new ServiceCollection()
            .AddSingleton(paramService)
            .BuildServiceProvider();

        var buildDefinition = A.Fake<BuildDefinition>(x => x.WithArgumentsForConstructor([services]));

        // Act
        buildDefinition.GetParam(parameterExpression);

        // Assert

        A
            .CallTo(() => paramService.GetParam(A<Expression<Func<string?>>>._, A<string?>._, A<Func<string?, string?>>._))
            .MustHaveHappened();
    }

    [Test]
    public void WriteVariable_CallsWorkflowVariableService()
    {
        // Arrange
        const string name = "name";
        const string value = "value";

        var workflowVariableService = A.Fake<IWorkflowVariableService>();

        A
            .CallTo(() => workflowVariableService.WriteVariable(name, value))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection()
            .AddSingleton(workflowVariableService)
            .BuildServiceProvider();

        var buildDefinition = A.Fake<BuildDefinition>(x => x.WithArgumentsForConstructor([services]));

        // Act
        buildDefinition.WriteVariable(name, value);

        // Assert
        A
            .CallTo(() => workflowVariableService.WriteVariable(name, value))
            .MustHaveHappened();
    }
}
