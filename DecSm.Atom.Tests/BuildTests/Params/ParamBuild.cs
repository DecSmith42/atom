namespace DecSm.Atom.Tests.BuildTests.Params;

[BuildDefinition]
public partial class ParamBuild : BuildDefinition, IParamTarget1, IParamTarget2
{
    public string? ExecuteValue { get; set; }
}

[TargetDefinition]
public partial interface IParamTarget1
{
    [ParamDefinition("param-1", "Param 1", "DefaultValue")]
    string Param1 => GetParam(() => Param1, "DefaultValue");

    string? ExecuteValue { get; set; }

    Target ParamTarget1 =>
        t => t.Executes(() =>
        {
            ExecuteValue = Param1;

            return Task.CompletedTask;
        });
}

[TargetDefinition]
public partial interface IParamTarget2
{
    [ParamDefinition("param-2", "Param 2")]
    string Param2 => GetParam(() => Param2)!;

    string? ExecuteValue { get; set; }

    Target ParamTarget2 =>
        t => t
            .RequiresParam(nameof(Param2))
            .Executes(() =>
            {
                ExecuteValue = Param2;

                return Task.CompletedTask;
            });
}
