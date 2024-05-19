var builder = AtomHost.CreateAtomBuilder<Build>(args, atom => atom.AddGithubWorkflows());

var app = builder.Build();

await app.RunAsync();