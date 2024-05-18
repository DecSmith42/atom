var builder = Host.CreateEmptyApplicationBuilder(new()
    { DisableDefaults = true, Args = args });

builder
    .AddAtom<Build>(args)
    .AddBatWorkflows();

var app = builder.Build();

await app.RunAsync();