namespace DecSm.Atom.Module.GithubWorkflows;

[TargetDefinition]
public partial interface IGithubHelper
{
    [SecretDefinition("github-token", "Github Token")]
    string GithubToken => GetParam(() => GithubToken)!;
}
