using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DecSm.Atom.SourceGenerators;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers")]
public static class Parser
{
    public static Dictionary<string, string> GetProjectsFromSolution(string solutionPath)
    {
        var solutionContents = File.ReadAllText(solutionPath);

        try
        {
            return solutionPath.EndsWith(".slnx")
                ? GetProjectsFromSlnx(solutionPath, solutionContents)
                : GetProjectsFromSln(solutionPath, solutionContents);
        }
        catch (Exception ex)
        {
            File.WriteAllText("L:\\err.txt", ex.ToString());

            throw;
        }
    }

    private static Dictionary<string, string> GetProjectsFromSlnx(string solutionPath, string solutionContents)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var document = XDocument.Parse(solutionContents);
        var solutionDirectory = Path.GetDirectoryName(solutionPath)!;

        var projectElements = document.Descendants("Project");

        foreach (var projectElement in projectElements)
        {
            var projectPath = projectElement.Attribute("Path")
                ?.Value;

            if (string.IsNullOrEmpty(projectPath))
                continue;

            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            var fullPath = Path.Combine(solutionDirectory, projectPath);

            result[projectName] = fullPath;
        }

        return result;
    }

    private static Dictionary<string, string> GetProjectsFromSln(string solutionPath, string solutionContents)
    {
        // Parse classic .sln files by scanning for Project(...) lines and extracting
        // the project name and relative path. Ignore solution folders and any entries
        // that do not point to a supported project file.

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Regex to capture: Project("{GUID}") = "Name", "RelativePath", "{GUID}"
        var projectLine = new Regex("""
                                    ^\s*Project\("\{[^}]+\}"\)\s*=\s*"(?<name>[^"]+)",\s*"(?<path>[^"]+)",\s*"\{[^}]+\}"
                                    """,
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        var solutionDirectory = Path.GetDirectoryName(solutionPath)!;

        using var reader = new StringReader(solutionContents);

        while (reader.ReadLine() is { } line)
        {
            // Quick filter
            if (line.IndexOf("Project(", StringComparison.Ordinal) < 0)
                continue;

            var match = projectLine.Match(line);

            if (!match.Success)
                continue;

            var name = match.Groups["name"].Value;
            var path = match.Groups["path"].Value;

            // Only consider typical .NET project types
            if (!(path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                  path.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase) ||
                  path.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase)))
                continue; // Likely a solution folder or non-project entry

            var fullPath = Path.IsPathRooted(path)
                ? path
                : Path.Combine(solutionDirectory, path);

            var projectName = string.IsNullOrWhiteSpace(name)
                ? Path.GetFileNameWithoutExtension(fullPath)
                : name;

            result[projectName] = fullPath;
        }

        return result;
    }
}
