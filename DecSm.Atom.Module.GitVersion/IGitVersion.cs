﻿namespace DecSm.Atom.Module.GitVersion;

[TargetDefinition]
public partial interface IGitVersion
{
    static void IBuildDefinition.Register(IServiceCollection services) =>
        services
            .AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>()
            .AddSingleton<IBuildVersionProvider, GitVersionBuildVersionProvider>();
}
