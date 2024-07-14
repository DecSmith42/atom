using DecSm.Atom.Reports;

namespace Atom.Targets;

[TargetDefinition]
internal partial interface IPackAtom : IDotnetPackHelper
{
    public const string AtomProjectName = "DecSm.Atom";

    Target PackAtom =>
        d => d
            .WithDescription("Builds the Atom project into a nuget package")
            .ProducesArtifact(AtomProjectName)
            .Executes(async () =>
            {
                await DotnetPackProject(AtomProjectName);

                // Test report data

                Logger.LogCritical("Critical log");

                try
                {
                    throw new UnreachableException("Unreachable message");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error log");
                }

                Logger.LogWarning(
                    "Warning log\nLorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Sed odio morbi quis commodo odio aenean sed. Malesuada pellentesque elit eget gravida cum. Ut lectus arcu bibendum at varius vel pharetra vel turpis. Tempus imperdiet nulla malesuada pellentesque elit eget");

                AddReportData(new ArtifactReportData("Test Artifact", "https://www.google.com"));

                AddReportData(new TableReportData([["Test1", "Value1"], ["Test2", "Value2"]])
                {
                    Title = "Test Report",
                    Header = ["Name", "Value"],
                });

                AddReportData(new ListReportData(new List<string>
                {
                    "Test1",
                    "Test2",
                })
                {
                    Title = "Test List",
                });

                AddReportData(new TextReportData(
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Sed odio morbi quis commodo odio aenean sed. Malesuada pellentesque elit eget gravida cum. Ut lectus arcu bibendum at varius vel pharetra vel turpis. Tempus imperdiet nulla malesuada pellentesque elit eget.")
                {
                    Title = "Test Text",
                });
            });
}