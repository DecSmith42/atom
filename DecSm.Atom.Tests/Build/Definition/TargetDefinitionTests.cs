namespace DecSm.Atom.Tests.Build.Definition;

[TestFixture]
public class TargetDefinitionTests
{
    [Test]
    public void WithDescription_SetsDescription()
    {
        // Arrange
        var description = "description";
        var targetDefinition = new TargetDefinition();

        // Act
        targetDefinition.WithDescription(description);

        // Assert
        targetDefinition.Description.ShouldBe(description);
    }

    [Test]
    public void Executes_SetsSingleTask()
    {
        // Arrange
        var task = Task.CompletedTask;
        var targetDefinition = new TargetDefinition();

        // Act
        targetDefinition.Executes(() => task);

        // Assert
        targetDefinition.Tasks.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]()
                .ShouldBe(task));
    }

    [Test]
    public void Executes_SetsMultipleTasks()
    {
        // Arrange
        var task1 = Task.CompletedTask;
        var task2 = Task.Delay(1);
        var targetDefinition = new TargetDefinition();

        // Act
        targetDefinition
            .Executes(() => task1)
            .Executes(() => task2);

        // Assert
        targetDefinition.Tasks.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(2),
            x => x[0]()
                .ShouldBe(task1),
            x => x[1]()
                .ShouldBe(task2));
    }

    [Test]
    public void Executes_DoesNotAddDuplicateTasks()
    {
        // Arrange
        var task = Task.CompletedTask;
        var targetDefinition = new TargetDefinition();

        // Act
        targetDefinition
            .Executes(() => task)
            .Executes(() => task);

        // Assert
        targetDefinition.Tasks.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]()
                .ShouldBe(task));
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private interface ITestTarget : IBuildDefinition
    {
        // ReSharper disable once UnusedMember.Local
#pragma warning disable CA1822
        public Target TestTarget => x => x;
#pragma warning restore CA1822
    }

    [Test]
    public void DependsOn_AddsDependency()
    {
        // Arrange
        var targetDefinition = new TargetDefinition();

        // Act
        targetDefinition.DependsOn<ITestTarget>();

        // Assert
        targetDefinition.Dependencies.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .ShouldBe(nameof(ITestTarget.TestTarget)));
    }

    [Test]
    public void RequiresParam_AddsRequiredParam()
    {
        // Arrange
        var paramName = "NotExpected";
        var targetDefinition = new TargetDefinition();

        // Act
        targetDefinition.RequiresParam(paramName);

        // Assert
        targetDefinition.RequiredParams.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .ShouldBe(nameof(paramName)));
    }

    [Test]
    public void ProducesArtifact_AddsProducedArtifact()
    {
        // Arrange
        var artifactName = "ArtifactName";
        var targetDefinition = new TargetDefinition();

        // Act
        targetDefinition.ProducesArtifact(artifactName);

        // Assert
        targetDefinition.ProducedArtifacts.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .ArtifactName
                .ShouldBe(artifactName));
    }

    [Test]
    public void ConsumesArtifact_AddsConsumedArtifact()
    {
        // Arrange
        var artifactName = "ArtifactName";
        var targetDefinition = new TargetDefinition();

        // Act
        targetDefinition.ConsumesArtifact<ITestTarget>(artifactName);

        // Assert
        targetDefinition.ConsumedArtifacts.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .ArtifactName
                .ShouldBe(artifactName),
            x => x[0]
                .TargetName
                .ShouldBe(nameof(ITestTarget.TestTarget)));
    }

    [Test]
    public void ProducesVariable_AddsProducedVariable()
    {
        // Arrange
        var variableName = "VariableName";
        var targetDefinition = new TargetDefinition();

        // Act
        targetDefinition.ProducesVariable(variableName);

        // Assert
        targetDefinition.ProducedVariables.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .ShouldBe(variableName));
    }

    [Test]
    public void ConsumesVariable_AddsConsumedVariable()
    {
        // Arrange
        var variableName = "VariableName";
        var targetDefinition = new TargetDefinition();

        // Act
        targetDefinition.ConsumesVariable<ITestTarget>(variableName);

        // Assert
        targetDefinition.ConsumedVariables.ShouldSatisfyAllConditions(x => x.ShouldNotBeEmpty(),
            x => x.Count.ShouldBe(1),
            x => x[0]
                .VariableName
                .ShouldBe(variableName),
            x => x[0]
                .TargetName
                .ShouldBe(nameof(ITestTarget.TestTarget)));
    }
}