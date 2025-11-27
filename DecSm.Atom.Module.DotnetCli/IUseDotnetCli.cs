using DecSm.Atom.Build;
using DecSm.Atom.Hosting;
using DecSm.Atom.Process;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantExtendsListEntry

namespace DecSm.Atom.Module.DotnetCli;

[PublicAPI]
[ConfigureHostBuilder]
public partial interface IUseDotnetCli : IBuildAccessor
{
    IDotnetCli DotnetCli => GetService<IDotnetCli>();

    protected static partial void ConfigureBuilder(IHostApplicationBuilder builder) =>
        builder.Services.AddSingleton<IDotnetCli, DotnetCli>();
}

[PublicAPI]
public partial interface IDotnetCli;

[PublicAPI]
internal partial class DotnetCli(IProcessRunner processRunner) : IDotnetCli;
