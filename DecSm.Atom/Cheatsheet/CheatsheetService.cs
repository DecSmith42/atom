namespace DecSm.Atom.Cheatsheet;

internal class CheatsheetService(IAnsiConsole console, BuildModel buildModel) : ICheatsheetService
{
    public void ShowCheatsheet()
    {
        console.WriteLine("Cheatsheet:");
        console.WriteLine();
        
        foreach (var target in buildModel.Targets)
        {
            console.WriteLine($"Target: {target.Name}");
            
            if (target.Dependencies.Count != 0)
                console.WriteLine($"  Dependencies: {string.Join(", ", target.Dependencies.Select(x => x.Name))}");
            
            if (target.RequiredParams.Count != 0)
                console.WriteLine($"  RequiredParams: {string.Join(", ", target.RequiredParams)}");
            
            console.WriteLine();
        }
    }
}