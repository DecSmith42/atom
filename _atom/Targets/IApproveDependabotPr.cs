using Octokit.GraphQL;
using Octokit.GraphQL.Internal;
using Octokit.GraphQL.Model;

namespace Atom.Targets;

public interface IApproveDependabotPr : IGithubHelper
{
    const string DependabotActorName = "dependabot[bot]";
    const string DependabotActorEmail = "dependabot[bot]@users.noreply.github.com";

    [ParamDefinition("pull-request-number", "The pull request number to approve.")]
    int PullRequestNumber => GetParam(() => PullRequestNumber);

    [SecretDefinition("gh-pullrequest-rw-token", "GitHub Pull Request Read/Write Token")]
    string? GithubPrRwToken => GetParam(() => GithubPrRwToken);

    Target ApproveDependabotPr =>
        t => t
            .RequiresParam(nameof(GithubPrRwToken), nameof(PullRequestNumber))
            .Executes(async cancellationToken =>
            {
                var actor = Github.Variables.Actor;

                var owner = Github.Variables.RepositoryOwner;

                var repo = Github.Variables
                    .Repository
                    .Split('/')
                    .Last();

                Logger.LogInformation("Github API action context: {Context}",
                    new
                    {
                        Actor = actor,
                        PullRequestNumber,
                        Owner = owner,
                        Repo = repo,
                    });

                if (actor != DependabotActorName)
                    throw new StepFailedException("Only pull requests from Dependabot can be auto-approved.");

                var productHeader = new ProductHeaderValue("Atom");
                var connection = new Connection(productHeader, new InMemoryCredentialStore(GithubPrRwToken));

                var prQuery = new Query()
                    .Repository(repo, owner)
                    .PullRequest(PullRequestNumber)
                    .Select(p => new
                    {
                        p.Id,
                        p.HeadRefOid,
                    })
                    .Compile();

                var prQueryResult = await connection.Run(prQuery, cancellationToken: cancellationToken);

                if (prQueryResult.Id.Value is null)
                    throw new StepFailedException("Could not find pull request.");

                var enableAutoMergeMutation = new Mutation()
                    .EnablePullRequestAutoMerge(new EnablePullRequestAutoMergeInput
                    {
                        PullRequestId = prQueryResult.Id,
                        AuthorEmail = DependabotActorEmail,
                        ExpectedHeadOid = prQueryResult.HeadRefOid,
                    })
                    .Select(x => new
                    {
                        x.ClientMutationId,
                    })
                    .Compile();

                var enableAutoMergeResult =
                    await connection.Run(enableAutoMergeMutation, cancellationToken: cancellationToken);

                if (enableAutoMergeResult is null)
                    throw new StepFailedException("Could not enable auto merge.");
            });
}
