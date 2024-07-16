namespace Atom.Targets.Sandbox;

[TargetDefinition]
internal partial interface IReport
{
    Target Report =>
        d => d.Executes(() =>
        {
            try
            {
                throw new("This is a test exception");
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "An error occurred");
                Logger.LogCritical(ex, "Another error occurred");
                Logger.LogCritical(ex, "Yet another error occurred");
            }

            Logger.LogError("This is an error message");
            Logger.LogError("This is another error message");
            Logger.LogError("This is a warning message");

            Logger.LogWarning("This is a warning message");
            Logger.LogWarning("This is another warning message");

            Logger.LogWarning("""
                              This is a long warning message:
                              Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
                              Sed odio morbi quis commodo odio aenean sed.
                              Malesuada pellentesque elit eget gravida cum.
                              Ut lectus arcu bibendum at varius vel pharetra vel turpis.
                              Tempus imperdiet nulla malesuada pellentesque elit eget.
                              """);

            AddReportData(new ArtifactReportData("Triangle", "https://placehold.co/300"));
            AddReportData(new ArtifactReportData("Square", "https://placehold.co/400"));
            AddReportData(new ArtifactReportData("Circle", "https://placehold.co/500"));

            AddReportData(new TableReportData([["Name", "Value"], ["Foo", "Really long value"], ["Baz", "Qux"]])
            {
                Title = "Table Report",
                Header = ["Name", "Value"],
                ColumnAlignments = [ColumnAlignment.Left, ColumnAlignment.Right],
                BeforeStandardData = false,
            });

            AddReportData(new TableReportData([["Name", "Value"], ["Foo", "Really long value"], ["Baz", "Qux"]])
            {
                ColumnAlignments = [ColumnAlignment.Right, ColumnAlignment.Left],
            });

            AddReportData(new ListReportData(["Foo", "Bar", "Baz", "Really long value"])
            {
                Title = "List Report",
            });

            AddReportData(new ListReportData(["Foo", "Bar", "Baz", "Really long value"]));

            AddReportData(new TextReportData("This is a text report")
            {
                Title = "Text Report",
            });

            AddReportData(new TextReportData("""
                                             Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
                                             Sed odio morbi quis commodo odio aenean sed.
                                             Malesuada pellentesque elit eget gravida cum.
                                             Ut lectus arcu bibendum at varius vel pharetra vel turpis.
                                             Tempus imperdiet nulla malesuada pellentesque elit eget.
                                             """));

            return Task.CompletedTask;
        });
}