var builder = AtomHost.CreateAtomBuilder<Build>(args, atom => atom.AddGithub());

var app = builder.Build();

await app.RunAsync();