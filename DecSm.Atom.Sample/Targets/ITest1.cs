namespace DecSm.Atom.Sample.Targets;

[TargetDefinition]
public partial interface ITest1
{
    [Param("my-param-1", "My param 1 description")]
    string? MyParam1 => GetParam(() => MyParam1);

    Target Test1 => d => d
        .Requires(() => MyParam1)
        .DependsOn<ITestDependency>()
        .Executes(() =>
        {
            Logger.LogInformation("Hello, '{MyParam1}', from ITest1!", MyParam1);
            return Task.CompletedTask;
        });
}