using DecSm.Atom.Build.Definition;
using DecSm.Atom.Hosting;
using DecSm.Atom.Module.LocalWorkflows;
using DecSm.Atom.Params;
using DecSm.Atom.Variables;
using DecSm.Atom.Workflows.Definition;
using DecSm.Atom.Workflows.Definition.Triggers;

namespace Atom;

[GenerateEntryPoint]
[BuildDefinition]
internal partial class Build : DefaultBuildDefinition, IPowershellWorkflows, ITargets
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("Test")
        {
            Triggers = [new ManualTrigger([ManualStringInput.ForParam(ParamDefinitions[nameof(ITargets.TestInput)])])],
            Targets = [Targets.SetupObjects, Targets.ConsumeObjects],
            WorkflowTypes = [new PowershellWorkflowType()],
        },
    ];
}

internal interface ITargets : IVariablesHelper
{
    [ParamDefinition("test-input", "Test input")]
    string? TestInput => GetParam(() => TestInput);

    [ParamDefinition("test-param", "Test parameter")]
    string? TestParam => GetParam(() => TestParam);

    Target SetupObjects =>
        t => t
            .DescribedAs("Setup objects")
            .RequiresParam(nameof(TestInput))
            .ProducesVariable(nameof(TestParam))
            .ProducesArtifact("TestArtifact")
            .Executes(async () =>
            {
                await WriteVariable(nameof(TestParam), "Test param 123");

                FileSystem.Directory.CreateDirectory(FileSystem.AtomPublishDirectory / "TestArtifact");
                await FileSystem.File.WriteAllTextAsync(FileSystem.AtomPublishDirectory / "TestArtifact" / "file", "Test artifact 123");
            });

    Target ConsumeObjects =>
        t => t
            .DescribedAs("Consume objects")
            .ConsumesVariable(nameof(SetupObjects), nameof(TestParam))
            .ConsumesArtifact(nameof(SetupObjects), "TestArtifact")
            .Executes(() =>
            {
                var testParam = TestParam;

                if (string.IsNullOrEmpty(testParam))
                    throw new InvalidOperationException("Test parameter is not set.");

                var testArtifact = FileSystem.File.ReadAllText(FileSystem.AtomArtifactsDirectory / "TestArtifact" / "file");

                if (testArtifact != "Test artifact 123")
                    throw new InvalidOperationException("Test artifact content is incorrect.");
            });
}
