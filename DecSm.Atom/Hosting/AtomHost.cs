namespace DecSm.Atom.Hosting;

[PublicAPI]
public static class AtomHost
{
    public static HostApplicationBuilder CreateAtomBuilder<T>(string[] args)
        where T : BuildDefinition
    {
        var builder = Host.CreateEmptyApplicationBuilder(new()
        {
            DisableDefaults = true,
            Args = args,
        });

        var environment = builder.Environment.EnvironmentName;

        builder.Configuration.AddJsonFile("appsettings.json", true, true);
        builder.Configuration.AddJsonFile($"appsettings.{environment}.json", true, true);

        builder.AddAtom<HostApplicationBuilder, T>(args);

        return builder;
    }

    public static void Run<T>(string[] args)
        where T : BuildDefinition =>
        CreateAtomBuilder<T>(args)
            .Build()
            .Run();
}
