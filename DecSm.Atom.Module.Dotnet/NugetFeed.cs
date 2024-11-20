namespace DecSm.Atom.Module.Dotnet;

public record NugetFeed(
    string Url,
    string? Name = null,
    string? Username = null,
    string? Password = null,
    string? PlainTextPassword = null
);
