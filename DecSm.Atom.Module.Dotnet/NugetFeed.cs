namespace DecSm.Atom.Module.Dotnet;

[PublicAPI]
public record NugetFeed
{
    public NugetFeed(
        string url,
        string? name = null,
        Func<string?>? username = null,
        Func<string?>? password = null,
        Func<string?>? plainTextPassword = null)
    {
        Url = url;
        Name = name;
        Username = username ?? (() => null);
        Password = password ?? (() => null);
        PlainTextPassword = plainTextPassword ?? (() => null);
    }

    public NugetFeed(
        string url,
        string? name = null,
        string? username = null,
        string? password = null,
        string? plainTextPassword = null)
    {
        Url = url;
        Name = name;
        Username = () => username;
        Password = () => password;
        PlainTextPassword = () => plainTextPassword;
    }

    public string Url { get; private init; }

    public string? Name { get; private init; }

    public Func<string?> Username { get; private init; }

    public Func<string?> Password { get; private init; }

    public Func<string?> PlainTextPassword { get; private init; }
}
