using DecSm.Atom.AzureKeyVault;

var builder = AtomHost.CreateAtomBuilder<Build>(args,
    atom => atom
        .AddGithubWorkflows()
        .AddAzureKeyVault());

var app = builder.Build();

await app.RunAsync();