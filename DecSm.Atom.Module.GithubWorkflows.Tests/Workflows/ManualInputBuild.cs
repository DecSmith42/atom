namespace DecSm.Atom.Module.GithubWorkflows.Tests.Workflows;

[BuildDefinition]
public partial class ManualInputBuild : BuildDefinition, IGithubWorkflows, IManualInputTarget
{
    public override IReadOnlyList<WorkflowDefinition> Workflows =>
    [
        new("manual-input-workflow")
        {
            Triggers =
            [
                new ManualTrigger
                {
                    Inputs =
                    [
                        ManualStringInput.ForParam(ParamDefinitions[Params.StringParamWithoutDefault]),
                        ManualStringInput.ForParam(ParamDefinitions[Params.StringParamWithDefault]),
                        ManualBoolInput.ForParam(ParamDefinitions[Params.BoolParamWithoutDefault]),
                        ManualBoolInput.ForParam(ParamDefinitions[Params.BoolParamWithDefault]),
                        ManualChoiceInput.ForParam(ParamDefinitions[Params.ChoiceParamWithoutDefault],
                            ["choice 1", "choice 2", "choice 3"]),
                        ManualChoiceInput.ForParam(ParamDefinitions[Params.ChoiceParamWithDefault],
                            ["choice 1", "choice 2", "choice 3"]),
                    ],
                },
            ],
            StepDefinitions = [Commands.ManualInputTarget],
            WorkflowTypes = [Github.WorkflowType],
        },
    ];
}

[TargetDefinition]
public partial interface IManualInputTarget
{
    [ParamDefinition("string-param-without-default", "String param")]
    string StringParamWithoutDefault => GetParam(() => StringParamWithoutDefault)!;

    [ParamDefinition("string-param-with-default", "String param", "default-value")]
    string StringParamWithDefault => GetParam(() => StringParamWithDefault)!;

    [ParamDefinition("bool-param-without-default", "Bool param")]
    bool BoolParamWithoutDefault => GetParam(() => BoolParamWithoutDefault);

    [ParamDefinition("bool-param-with-default", "Bool param", "true")]
    bool BoolParamWithDefault => GetParam(() => BoolParamWithDefault);

    [ParamDefinition("choice-param-without-default", "Choice param")]
    string ChoiceParamWithoutDefault => GetParam(() => ChoiceParamWithoutDefault)!;

    [ParamDefinition("choice-param-with-default", "Choice param", "choice 1")]
    string ChoiceParamWithDefault => GetParam(() => ChoiceParamWithDefault)!;

    Target ManualInputTarget => d => d;
}
