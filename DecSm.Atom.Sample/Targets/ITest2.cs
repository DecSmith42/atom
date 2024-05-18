namespace DecSm.Atom.Sample.Targets;

[TargetDefinition]
public partial interface ITest2
{
    [Param("my-param-2", "My param 2 description")]
    string? MyParam2 => GetParam(() => MyParam2);

    Target Test2 => d => d
        .DependsOn<ITest1>()
        .Executes(_ =>
        {
            Logger.LogInformation("Hello, '{MyParam2}'!", MyParam2);
            return Task.CompletedTask;
        });
}