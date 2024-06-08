var builder = AtomHost.CreateAtomBuilder<Build>(args,
    atom => atom
        .AddGithubWorkflows()
        .AddAzureKeyVault());

builder.Services.AddSingleton<IArtifactProvider, FileArtifactProvider>();

var app = builder.Build();

await app.RunAsync();