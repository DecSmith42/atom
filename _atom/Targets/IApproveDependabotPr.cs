using Octokit.GraphQL;
using Octokit.GraphQL.Internal;
using Octokit.GraphQL.Model;
using Environment = System.Environment;

namespace Atom.Targets;

public interface IApproveDependabotPr : IGithubHelper
{
    Target ApprovePr =>
        t => t
            .RequiresParam(nameof(GithubToken))
            .Executes(async cancellationToken =>
            {
                if (Github.Variables.Actor != "dependabot[bot]")
                    throw new StepFailedException("Only pull requests from Dependabot can be auto-approved.");

                var pullRequestNumberVariable = Environment.GetEnvironmentVariable("GITHUB_EVENT_NUMBER");

                if (pullRequestNumberVariable is not { Length: > 0 } ||
                    !int.TryParse(pullRequestNumberVariable, out var prNumber))
                    throw new StepFailedException("Could not determine pull request number from environment.");

                const string owner = "DecSmith42";
                const string repo = "atom";

                var clientMutationId =
                    $"atom-{Environment.MachineName.ToLowerInvariant().Replace(" ", "-")}-{Environment.ProcessId}";

                var productHeader = new ProductHeaderValue("YourAppName");
                var connection = new Connection(productHeader, new InMemoryCredentialStore(new(GithubToken)));

                var prQuery = new Query()
                    .Repository(repo, owner)
                    .PullRequest(prNumber)
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

                var approvePrMutation = new Mutation().AddPullRequestReview(new AddPullRequestReviewInput
                {
                    ClientMutationId = clientMutationId,
                    PullRequestId = prQueryResult.Id,
                    Event = PullRequestReviewEvent.Approve,
                    Body = "Automated approval by build system.",
                });

                var approvePrResult = await connection.Run(approvePrMutation, cancellationToken);

                if (approvePrResult is null)
                    throw new StepFailedException("Could not approve pull request.");
            });
}
