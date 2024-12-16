namespace DecSm.Atom.Tests.Utils;

public static class Initializer
{
    [ModuleInitializer]
    public static void Init()
    {
        DiffTools.UseOrder(DiffTool.Rider, DiffTool.VisualStudioCode, DiffTool.VisualStudio, DiffTool.BeyondCompare);
        VerifierSettings.IgnoreMember<BuildModel>(x => x.DeclaringAssembly);
        VerifierSettings.IgnoreMember<TargetModel>(x => x.DeclaringAssembly);
    }
}
