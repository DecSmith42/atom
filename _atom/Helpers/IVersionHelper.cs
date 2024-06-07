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
            
            var packageVersion = GetPackageVersionComponent(text);
            
            if (packageVersion != null)
                return packageVersion;
        }
        
        throw new InvalidOperationException("Could not find PackageVersion in .csproj or any Directory.Build.props file");
    }
    
    string? GetPackageVersionComponent(string text)
    {
        var packageVersionMatch = Regex.Match(text, "<PackageVersion>(.*)</PackageVersion>");
        
        if (packageVersionMatch is { Success: true, Groups.Count: > 1 })
            return packageVersionMatch
                .Groups[1]
                .Value
                .Replace("$(Version)", GetVersionComponent(text))
                .Replace("$(VersionPrefix)", GetVersionPrefixComponent(text))
                .Replace("$(VersionSuffix)", GetVersionSuffixComponent(text));
        
        return null;
    }
    
    string? GetVersionComponent(string text)
    {
        var versionMatch = Regex.Match(text, "<Version>(.*)</Version>");
        
        if (versionMatch is { Success: true, Groups.Count: > 1 })
            return versionMatch
                .Groups[1]
                .Value
                .Replace("$(VersionPrefix)", GetVersionPrefixComponent(text))
                .Replace("$(VersionSuffix)", GetVersionSuffixComponent(text));
        
        return null;
    }
    
    string GetVersionPrefixComponent(string text)
    {
        var versionMatch = Regex.Match(text, "<VersionPrefix>(.*)</VersionPrefix>");
        
        if (versionMatch is { Success: true, Groups.Count: > 1 })
            return versionMatch.Groups[1].Value;
        
        return string.Empty;
    }
    
    string GetVersionSuffixComponent(string text)
    {
        var versionMatch = Regex.Match(text, "<VersionSuffix>(.*)</VersionSuffix>");
        
        if (versionMatch is { Success: true, Groups.Count: > 1 })
            return versionMatch.Groups[1].Value;
        
        return string.Empty;
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
            var versionMatch = Regex.Match(text, "<Version>(.*)</Version>");
            
            if (versionMatch is { Success: true, Groups.Count: > 1 })
                return versionMatch.Groups[1].Value;
        }
        
        throw new InvalidOperationException("Could not find Version in .csproj or any Directory.Build.props file");
    }
}