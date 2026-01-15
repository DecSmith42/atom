using Octokit.GraphQL;
using Octokit.GraphQL.Internal;
using Octokit.GraphQL.Model;
using Environment = System.Environment;

namespace Atom.Targets;

public interface IApproveDependabotPr : IGithubHelper
{
    [ParamDefinition("pull-request-number", "The pull request number to approve.")]
    int PullRequestNumber => GetParam(() => PullRequestNumber);

    Target ApproveDependabotPr =>
        t => t
            .RequiresParam(nameof(GithubToken), nameof(PullRequestNumber))
            .Executes(async cancellationToken =>
            {
                var owner = Github.Variables.RepositoryOwner;

                var repo = Github.Variables
                    .Repository
                    .Split('/')
                    .Last();

                var clientMutationId =
                    $"atom-{Environment.MachineName.ToLowerInvariant().Replace(" ", "-")}-{Environment.ProcessId}";

                Logger.LogInformation("Github API action context: {Context}",
                    new
                    {
                        PullRequestNumber,
                        Owner = owner,
                        Repo = repo,
                        ClientMutationId = clientMutationId,
                        Github.Variables.Actor,
                    });

                if (Github.Variables.Actor != "dependabot[bot]")
                    throw new StepFailedException("Only pull requests from Dependabot can be auto-approved.");

                var productHeader = new ProductHeaderValue("Atom");
                var connection = new Connection(productHeader, new InMemoryCredentialStore(new(GithubToken)));

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

                var enableAutoMergeMutation = new Mutation().EnablePullRequestAutoMerge(
                    new EnablePullRequestAutoMergeInput
                    {
                        ClientMutationId = clientMutationId,
                        PullRequestId = prQueryResult.Id,
                        AuthorEmail = "dependabot[bot]@users.noreply.github.com",
                        ExpectedHeadOid = prQueryResult.HeadRefOid,
                    });

                var enableAutoMergeResult = await connection.Run(enableAutoMergeMutation, cancellationToken);

                if (enableAutoMergeResult is null)
                    throw new StepFailedException("Could not enable auto merge.");
            });
}
