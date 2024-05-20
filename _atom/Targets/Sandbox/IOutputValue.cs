namespace Atom.Targets.Sandbox;

[TargetDefinition]
public partial interface IOutputValue
{
    [Param("test-value-1", "Test value to be passed between jobs")]
    string TestValue1 => GetParam(() => TestValue1)!;
    
    Target OutputValue =>
        d => d
            .ProducesVariable(Build.Params.TestValue1)
            .Executes(() => WriteVariable(Build.Params.TestValue1, "WOLOLO"));
}