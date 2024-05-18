var builder = Host.CreateEmptyApplicationBuilder(new()
{
    DisableDefaults = true,
    Args = args,
});

builder.Configuration.AddJsonFile("appsettings.json", true, true);

builder
    .AddAtom<Build>(args)
    .AddBatWorkflows();

var app = builder.Build();

await app.RunAsync();