namespace DecSm.Atom.Hosting;

public interface IConfigureHost
{
    void ConfigureBuildHostBuilder(IHostApplicationBuilder builder);

    void ConfigureBuildHost(IHost host);
}
