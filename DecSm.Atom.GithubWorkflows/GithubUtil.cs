namespace DecSm.Atom.GithubWorkflows;

public static class GithubUtil
{
    public static class Runner
    {
        public static string Name { get; } = Environment.GetEnvironmentVariable("RUNNER_NAME") ?? string.Empty;
        
        public static string Os { get; } = Environment.GetEnvironmentVariable("RUNNER_OS") ?? string.Empty;
        
        public static string Architecture { get; } = Environment.GetEnvironmentVariable("RUNNER_ARCHITECTURE") ?? string.Empty;
    }
}