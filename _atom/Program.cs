using DecSm.Atom.Build;

var builder = AtomHost.CreateAtomBuilder<Build>(args,
    atom => atom
        .AddGithubWorkflows()
        .AddAzureKeyVault()
        .AddAzureArtifacts());

builder.Services.AddSingleton<IProcessRunner, ProcessRunner>();
builder.Services.AddSingleton<IBuildIdProvider, GitVersionBuildIdProvider>();

var app = builder.Build();

await app.RunAsync();