namespace DecSm.Atom.Hosting;

/// <summary>
///     Provides methods for creating and running Atom-based host applications.
/// </summary>
/// <remarks>
///     This class simplifies the setup of an Atom host environment, configuring
///     essential application settings and initialization based on provided build definitions.
/// </remarks>
/// <example>
///     Example usage:
///     <code>
/// AtomHost.Run&lt;MyBuildDefinition&gt;(args);
/// </code>
/// </example>
[PublicAPI]
public static class AtomHost
{
    /// <summary>
    ///     Creates and configures an instance of <see cref="HostApplicationBuilder" />
    ///     tailored for Atom applications with the specified build definition.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of <see cref="BuildDefinition" /> used to configure the Atom host.
    /// </typeparam>
    /// <param name="args">Command-line arguments passed to the application.</param>
    /// <returns>
    ///     A configured <see cref="HostApplicationBuilder" /> ready to build and run the application.
    /// </returns>
    public static HostApplicationBuilder CreateAtomBuilder<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string[] args)
        where T : BuildDefinition
    {
        var builder = Host.CreateEmptyApplicationBuilder(new()
        {
            DisableDefaults = true,
            Args = args,
            EnvironmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                              Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            ApplicationName = "Atom",
        });

        var environment = builder.Environment.EnvironmentName;

        builder.Configuration.AddJsonFile("appsettings.json", true, true);

        if (environment.Length > 0)
            builder.Configuration.AddJsonFile($"appsettings.{environment}.json", true, true);

        builder.AddAtom<HostApplicationBuilder, T>(args);

        return builder;
    }

    /// <summary>
    ///     Builds and runs an Atom-based application using the specified build definition.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of <see cref="BuildDefinition" /> used to configure and run the Atom application.
    /// </typeparam>
    /// <param name="args">Command-line arguments passed to the application.</param>
    /// <remarks>
    ///     This method configures, builds, and immediately runs the host application.
    /// </remarks>
    /// <example>
    ///     Example usage:
    ///     <code>
    /// AtomHost.Run&lt;MyBuildDefinition&gt;(args);
    /// </code>
    /// </example>
    public static void Run<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        string[] args)
        where T : BuildDefinition =>
        CreateAtomBuilder<T>(args)
            .Build()
            .UseAtom()
            .Run();
}
