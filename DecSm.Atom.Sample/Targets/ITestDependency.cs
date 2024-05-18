namespace DecSm.Atom.Sample.Targets;

[TargetDefinition]
public partial interface ITestDependency
{
    [Param("my-param-2", "My param 2 description")]
    string? MyParam2 => GetParam(() => MyParam2);
    
    Target TestDependency =>
        d => d
        .Executes(_ =>
        {
            Logger.LogInformation("Hello, '{MyParam2}', from ITestDependency!", MyParam2);
            
            return Task.CompletedTask;
        });
}