using DecSm.Atom.Build;
using DecSm.Atom.Hosting;
using DecSm.Atom.Process;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ReSharper disable PartialTypeWithSinglePart

namespace DecSm.Atom.Module.DotnetCli;

[PublicAPI]
[ConfigureHostBuilder]
public partial interface IUseDotnetCli : IBuildAccessor
{
    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IDotnetCli, DotnetCli>();

    IDotnetCli DotnetCli => GetService<IDotnetCli>();
}

[PublicAPI]
public partial interface IDotnetCli;

[PublicAPI]
internal partial class DotnetCli(ProcessRunner processRunner) : IDotnetCli;
