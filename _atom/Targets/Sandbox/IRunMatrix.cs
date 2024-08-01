namespace Atom.Targets.Sandbox;

[TargetDefinition]
internal partial interface IRunMatrix
{
    [ParamDefinition("matrix-val-1", "Matrix value 1")]
    string MatrixVal1 => GetParam(() => MatrixVal1)!;

    [ParamDefinition("matrix-val-2", "Matrix value 2")]
    string MatrixVal2 => GetParam(() => MatrixVal2)!;

    Target RunMatrix =>
        d => d.Executes(() =>
        {
            Logger.LogInformation("Matrix: {MatrixVal1} x {MatrixVal2}", MatrixVal1, MatrixVal2);

            return Task.CompletedTask;
        });
}