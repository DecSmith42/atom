namespace Atom.Targets.Sandbox;

[TargetDefinition]
public partial interface IInputValue : IOutputValue
{
    Target InputValue =>
        d => d
            .ConsumesVariable<IOutputValue>(Build.Params.TestValue1)
            .Executes(() =>
            {
                Logger.LogInformation("TestValue1: {TestValue1}", TestValue1);

                return Task.CompletedTask;
            });
}