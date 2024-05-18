namespace DecSm.Atom.Cheatsheet;

public class CheatsheetService(IAnsiConsole console, IAtomBuild atomBuild) : ICheatsheetService
{
    public void ShowCheatsheet()
    {
        var plan = atomBuild.ExecutionPlan;

        console.WriteLine("Cheatsheet:");
        console.WriteLine();

        foreach (var target in plan.Targets)
        {
            console.WriteLine($"Target: {target.TargetDefinition.Name}");

            if (target.Dependencies.Count != 0)
                console.WriteLine($"  Dependencies: {string.Join(", ", target.Dependencies.Select(x => x.TargetDefinition.Name))}");

            if (target.Dependents.Count != 0)
                console.WriteLine($"  Dependents: {string.Join(", ", target.Dependents.Select(x => x.TargetDefinition.Name))}");

            if (target.TargetDefinition.Requirements.Count != 0)
                console.WriteLine($"  Requirements: {string.Join(", ", target.TargetDefinition.Requirements)}");

            console.WriteLine();
        }
    }
}