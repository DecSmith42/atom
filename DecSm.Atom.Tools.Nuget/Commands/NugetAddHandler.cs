namespace DecSm.Atom.Tools.Nuget.Commands;

public static class NugetAddHandler
{
    public static async Task<int> Handle(NugetAddOptions options, CancellationToken cancellationToken)
    {
        var validateResult = options.Validate();

        if (validateResult is not 0)
            return validateResult;

        Console.WriteLine("Fetching nuget sources...");

        var listSourceProcess = Process.Start(new ProcessStartInfo("dotnet", "nuget list source")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        })!;

        await listSourceProcess.WaitForExitAsync(cancellationToken);
        var listSourceOutput = await listSourceProcess.StandardOutput.ReadToEndAsync(cancellationToken);
        var listSourceError = await listSourceProcess.StandardError.ReadToEndAsync(cancellationToken);

        if (listSourceProcess.ExitCode is not 0)
        {
            Console.WriteLine("Failed to list nuget sources.");
            Console.WriteLine(listSourceOutput);
            Console.WriteLine(listSourceError);

            return 1;
        }

        var feed = options.GetFeed();

        if (listSourceOutput.Contains(feed.Name) || listSourceOutput.Contains(feed.Url))
        {
            Console.WriteLine($"'{feed.Name}' feed is already present, skipping");

            return 0;
        }

        var secret = Environment.GetEnvironmentVariable($"NUGET_TOKEN_{feed.Name.Replace(" ", "_").ToUpper()}");

        // Sanitize feed name and url
        var feedName = new string(feed.Name.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
        var feedUrl = new string(feed.Url.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '/' || c == ':' || c == '.').ToArray());

        // Sanitize secret
        secret = secret
            ?.Replace("\"", "")
            .Replace("\n", "")
            .Replace("\r", "") ??
                 string.Empty;

        Console.WriteLine($"Adding {feed.Name} feed...");

        var addSourceProcess = Process.Start(new ProcessStartInfo("dotnet")
        {
            ArgumentList = { "nuget", "add", "source", "--name", feedName, "--username", "USERNAME", "--password", secret, "--store-password-in-clear-text", feedUrl },
            RedirectStandardError = true,
        })!;

        await addSourceProcess.WaitForExitAsync(cancellationToken);

        var addSourceError = await addSourceProcess.StandardError.ReadToEndAsync(cancellationToken);

        if (addSourceProcess.ExitCode is 0)
        {
            Console.WriteLine($"'{feed.Name}' feed added successfully.");

            return 0;
        }

        Console.WriteLine($"Failed to add {feed.Name} feed.");
        Console.WriteLine(addSourceError);

        return 1;
    }
}
