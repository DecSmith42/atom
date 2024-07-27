var builder = AtomHost.CreateAtomBuilder<Build>(args,
    atom => atom
        .AddGithubWorkflows()
        .AddAzureKeyVault()
        .AddAzureArtifacts());


var app = builder.Build();

await app.RunAsync();