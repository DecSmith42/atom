namespace DecSm.Atom.Tests.BuildTests.Targets;

[BuildDefinition]
public partial class ConfigureBuilderAndHostBuild : BuildDefinition,
    ITargetWithConfigureBuilder,
    ITargetWithConfigureBuilderAndConfigureHost
{
    public bool IsSetupExecuted2 { get; set; }
}

[ConfigureHostBuilder]
public partial interface ITargetWithConfigureBuilder
{
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("SetupExecuted1", "true")]);
}

[ConfigureHostBuilder]
[ConfigureHost]
public partial interface ITargetWithConfigureBuilderAndConfigureHost
{
    public bool IsSetupExecuted2 { get; set; }

    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Configuration.AddInMemoryCollection([new("SetupExecuted2", "true")]);

    protected static partial void ConfigureHost(IHost host) =>
        ((ITargetWithConfigureBuilderAndConfigureHost)host.Services.GetRequiredService<IBuildDefinition>()).IsSetupExecuted2 = true;
}
