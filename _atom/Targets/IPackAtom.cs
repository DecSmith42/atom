namespace Targets;

[TargetDefinition]
public partial interface IPackAtom
{
    string AtomProjectName => "DecSm.Atom";
    
    Target PackAtom =>
        d => d.Executes(async _ =>
        {
            Logger.LogInformation("Packing Atom project {AtomProjectName}", AtomProjectName);
            
            var fs = Services.GetRequiredService<IFileSystem>();
            var project = fs.FileInfo.New(fs.AtomRoot() / AtomProjectName / $"{AtomProjectName}.csproj");
            
            if (!project.Exists)
                throw new InvalidOperationException($"Project file {project.FullName} does not exist.");
            
            var process = Process.Start(new ProcessStartInfo("dotnet")
            {
                Arguments = $"pack {project.FullName}",
            });
            
            await (process?.WaitForExitAsync() ?? Task.CompletedTask);
            
            if (process?.ExitCode != 0)
                throw new InvalidOperationException($"Failed to pack Atom project {AtomProjectName}.");
            
            Logger.LogInformation("Packed Atom project {AtomProjectName}", AtomProjectName);
        });
}