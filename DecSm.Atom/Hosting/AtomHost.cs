﻿namespace DecSm.Atom.Hosting;

public static class AtomHost
{
    public static HostApplicationBuilder CreateAtomBuilder<T>(string[] args, Action<IAtomConfiguration>? configureAtom = null)
        where T : BuildDefinition
    {
        var builder = Host.CreateEmptyApplicationBuilder(new()
        {
            DisableDefaults = true,
            Args = args,
        });

        builder.Configuration.AddJsonFile("appsettings.json", true, true);
        builder.Configuration.AddUserSecrets(Assembly.GetEntryAssembly()!);

        if (configureAtom is not null)
            configureAtom.Invoke(builder.AddAtom<T>(args));
        else
            builder.AddAtom<T>(args);

        return builder;
    }

    public static void Run<T>(string[] args)
        where T : BuildDefinition =>
        CreateAtomBuilder<T>(args)
            .Build()
            .Run();
}