namespace DecSm.Atom.Tests.Utils;

public static class TestUtils
{
    public static IHost CreateTestHost<T>(
        TestConsole? console = null,
        MockFileSystem? fileSystem = null,
        CommandLineArgs? commandLineArgs = null,
        Action<HostApplicationBuilder>? configure = null)
        where T : BuildDefinition
    {
        var builder = AtomHost.CreateAtomBuilder<T>([]);

        console ??= new();

        fileSystem ??= new(new Dictionary<string, MockFileData>
            {
                { @"C:\Atom\Atom.sln", new(string.Empty) },
                { @"C:\Atom\_atom\_atom.csproj", new(string.Empty) },
            },
            @"C:\Atom\_atom");

        commandLineArgs ??= new(true, []);

        builder.Services.AddSingleton<IAnsiConsole>(console);
        builder.Services.AddKeyedSingleton<IFileSystem>("RootFileSystem", fileSystem);
        builder.Services.AddSingleton(commandLineArgs);

        configure?.Invoke(builder);

        return builder.Build();
    }
}
