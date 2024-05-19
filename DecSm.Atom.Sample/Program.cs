var builder = AtomHost.CreateAtomBuilder<Build>(args, atom => atom.AddBatWorkflows());

var app = builder.Build();

await app.RunAsync();