namespace DecSm.Atom;

[TargetDefinition]
public partial interface IValidateBuild : IReportsHelper
{
    Target ValidateBuild =>
        t => t
            .DescribedAs("Checks the atom build for common issues.")
            .Executes(() =>
            {
                // ReSharper disable once CollectionNeverUpdated.Local - TODO
                var errors = new List<string>();
                var warnings = new List<string>();

                var build = GetService<BuildModel>();

                var targets = build.Targets;

                warnings.AddRange(targets
                    .Where(x => x.Description is not { Length: > 0 })
                    .Select(x => $"Target '{x.Name}' has no description."));

                if (warnings.Count > 0)
                    AddReportData(new ListReportData(warnings)
                    {
                        Title = "Warnings",
                    });

                // ReSharper disable once InvertIf
                if (errors.Count > 0)
                {
                    AddReportData(new ListReportData(errors)
                    {
                        Title = "Errors",
                    });

                    throw new StepFailedException("Validation failed.");
                }

                return Task.CompletedTask;
            });
}
