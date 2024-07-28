var builder = AtomHost.CreateAtomBuilder<Build>(args,
    atom => atom
        .AddGithubWorkflows()
        .AddAzureKeyVault()
        .AddAzureArtifacts()
        .AddGitVersioning());

var app = builder.Build();

await app.RunAsync();