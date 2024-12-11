namespace DecSm.Atom.Tests.Utils;

public static class TestUtils
{
    public static IHost CreateTestHost<T>(
        TestConsole? console = null,
        MockFileSystem? fileSystem = null,
        CommandLineArgs? commandLineArgs = null,
        TestBuildIdProvider? buildIdProvider = null,
        TestBuildVersionProvider? buildVersionProvider = null,
        Action<HostApplicationBuilder>? configure = null)
        where T : BuildDefinition
    {
        var builder = AtomHost.CreateAtomBuilder<T>([]);

        console ??= new();

        fileSystem ??= Environment.OSVersion.Platform is PlatformID.Win32NT
            ? new(new Dictionary<string, MockFileData>
                {
                    { @"C:\Atom\Atom.sln", new(string.Empty) },
                    { @"C:\Atom\_atom\_atom.csproj", new(string.Empty) },
                },
                @"C:\Atom\_atom")
            : new(new Dictionary<string, MockFileData>
                {
                    { "/Atom/Atom.sln", new(string.Empty) },
                    { "/Atom/_atom/_atom.csproj", new(string.Empty) },
                },
                "/Atom/_atom");

        commandLineArgs ??= new(true, []);

        buildIdProvider ??= new();

        buildVersionProvider ??= new();

        builder.Services.AddKeyedSingleton<IAnsiConsole>("StaticAccess", console);
        builder.Services.AddKeyedSingleton<IFileSystem>("RootFileSystem", fileSystem);
        builder.Services.AddSingleton(commandLineArgs);
        builder.Services.AddSingleton(buildIdProvider);
        builder.Services.AddSingleton(buildVersionProvider);

        configure?.Invoke(builder);

        return builder.Build();
    }
}
