namespace DecSm.Atom.Hosting;

/// <summary>
///     Provides the entry point for configuring the host within a build.
///     Automatically added to the build definition via source generation,
///     if the build definition inherits from any target definitions or helpers that
///     implement <see cref="ConfigureHostAttribute" /> or <see cref="ConfigureHostBuilderAttribute" />.
///     Should only be used internally by the build system.
/// </summary>
/// <remarks>
///     Should not be used directly in application code.
/// </remarks>
public interface IConfigureHost
{
    void ConfigureBuildHostBuilder(IHostApplicationBuilder builder);

    void ConfigureBuildHost(IHost host);
}
