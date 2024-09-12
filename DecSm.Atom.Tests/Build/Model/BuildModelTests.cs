namespace DecSm.Atom.Tests.Build.Model;

[TestFixture]
public class BuildModelTests
{
    [Test]
    public void CurrentTarget_WhenNoTargets_ReturnsNull()
    {
        // Arrange
        var buildModel = new BuildModel
        {
            Targets = [],
            TargetStates = new Dictionary<TargetModel, TargetState>(),
        };

        // Act
        var currentTarget = buildModel.CurrentTarget;

        // Assert
        currentTarget.ShouldBeNull();
    }

    private TargetModel TestTargetModel =>
        new("TargetModel", null, false)
        {
            Tasks = [],
            RequiredParams = [],
            ConsumedArtifacts = [],
            ProducedArtifacts = [],
            ConsumedVariables = [],
            ProducedVariables = [],
            Dependencies = [],
        };

    [Test]
    public void CurrentTarget_WhenNoRunningTargets_ReturnsNull()
    {
        // Arrange
        var targetModel = TestTargetModel;

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    targetModel, new(targetModel.Name)
                    {
                        Status = TargetRunState.Uninitialized,
                    }
                },
            },
        };

        // Act
        var currentTarget = buildModel.CurrentTarget;

        // Assert
        currentTarget.ShouldBeNull();
    }

    [Test]
    public void CurrentTarget_WhenRunningTarget_ReturnsTarget()
    {
        // Arrange
        var targetModel = TestTargetModel;

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    targetModel, new(targetModel.Name)
                    {
                        Status = TargetRunState.Running,
                    }
                },
            },
        };

        // Act
        var currentTarget = buildModel.CurrentTarget;

        // Assert
        currentTarget.ShouldBe(targetModel);
    }

    [Test]
    public void CurrentTarget_WhenMultipleRunningTargets_ReturnsFirstTarget()
    {
        // Arrange
        var targetModel1 = TestTargetModel;

        var targetModel2 = TestTargetModel with
        {
            Name = "TargetModel2",
        };

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel1,
                targetModel2,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>
            {
                {
                    targetModel1, new(targetModel1.Name)
                    {
                        Status = TargetRunState.Running,
                    }
                },
                {
                    targetModel2, new(targetModel2.Name)
                    {
                        Status = TargetRunState.Running,
                    }
                },
            },
        };

        // Act
        var currentTarget = buildModel.CurrentTarget;

        // Assert
        currentTarget.ShouldBe(targetModel1);
    }

    [Test]
    public void GetTarget_WhenTargetExists_ReturnsTarget()
    {
        // Arrange
        var targetModel = TestTargetModel;

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>(),
        };

        // Act
        var target = buildModel.GetTarget(targetModel.Name);

        // Assert
        target.ShouldBe(targetModel);
    }

    [Test]
    public void GetTarget_WhenTargetDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var targetModel = TestTargetModel;

        var buildModel = new BuildModel
        {
            Targets = new List<TargetModel>
            {
                targetModel,
            },
            TargetStates = new Dictionary<TargetModel, TargetState>(),
        };

        // Act
        void Act()
        {
            buildModel.GetTarget("NonExistentTarget");
        }

        // Assert
        Assert.Throws<ArgumentException>(Act);
    }
}
