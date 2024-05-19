namespace Atom.Helpers;

[TargetDefinition]
public partial interface IVersionHelper
{
    string GetProjectPackageVersion(AbsolutePath project)
    {
        List<AbsolutePath> projectPropertyFiles = [project];
        
        var path = project.Parent;
        
        while (path?.Parent != null)
        {
            projectPropertyFiles.AddRange(FileSystem
                .Directory
                .GetFiles(path, "Directory.Build.props")
                .Select(x => path / x));
            
            if (path == FileSystem.SolutionRoot())
                break;
            
            path = path.Parent;
        }
        
        foreach (var projectPropertyFile in projectPropertyFiles)
        {
            var text = FileSystem.File.ReadAllText(projectPropertyFile);
            
            // First, look for PackageVersion
            var match = Regex.Match(text, "<PackageVersion>(.*)</PackageVersion>");
            
            if (match is { Success: true, Groups.Count: > 1 } && match.Groups[1].Value != "$(Version)")
                return match.Groups[1].Value;
            
            // If not found, look for Version
            match = Regex.Match(text, "<Version>(.*)</Version>");
            
            if (match is { Success: true, Groups.Count: > 1 })
                return match.Groups[1].Value;
        }
        
        throw new InvalidOperationException("Could not find PackageVersion in .csproj or any Directory.Build.props file");
    }
    
    string GetProjectBaseVersion(AbsolutePath project)
    {
        List<AbsolutePath> projectPropertyFiles = [project];
        
        var path = project.Parent;
        
        while (path?.Parent != null)
        {
            projectPropertyFiles.AddRange(FileSystem
                .Directory
                .GetFiles(path, "Directory.Build.props")
                .Select(x => path / x));
            
            if (path == FileSystem.SolutionRoot())
                break;
            
            path = path.Parent;
        }
        
        foreach (var projectPropertyFile in projectPropertyFiles)
        {
            var text = FileSystem.File.ReadAllText(projectPropertyFile);
            
            // Look for Version
            var match = Regex.Match(text, "<Version>(.*)</Version>");
            
            if (match is { Success: true, Groups.Count: > 1 })
                return match.Groups[1].Value;
        }
        
        throw new InvalidOperationException("Could not find Version in .csproj or any Directory.Build.props file");
    }
}