namespace DecSm.Atom.Util;

/// <summary>
///     Extension methods that add robust retry behavior to Task and Task&lt;T&gt; operations.
/// </summary>
[PublicAPI]
public static class TaskExtensions
{
    /// <summary>
    ///     Retries awaiting the provided <see cref="Task" /> when it faults, up to the specified number of retries.
    /// </summary>
    /// <param name="task">The task to await. If <c>null</c>, a completed task is returned.</param>
    /// <param name="retryCount">The number of retries after the initial attempt. Must be zero or greater.</param>
    /// <param name="retryDelay">The delay between attempts.</param>
    /// <returns>A task that completes when the underlying task completes successfully, or throws after the final attempt.</returns>
    /// <remarks>
    ///     Exceptions from failed attempts are aggregated and thrown as an <see cref="AggregateException" /> after the final attempt.
    ///     Cancellation-related exceptions are rethrown immediately and are not retried.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="retryCount" /> is negative.</exception>
    /// <exception cref="OperationCanceledException">Rethrown if the operation is canceled.</exception>
    /// <exception cref="StackOverflowException">Rethrown without retry.</exception>
    public static Task WithRetry(this Task? task, int retryCount = 5, TimeSpan retryDelay = default)
    {
        if (task is null)
            return Task.CompletedTask;

        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);

        return Task.Run(async () =>
        {
            Exception? exception = null;

            for (var attempt = 0; attempt <= retryCount; attempt++)
                try
                {
                    await task.ConfigureAwait(false);

                    return;
                }
                catch (StackOverflowException)
                {
                    throw;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    exception = exception switch
                    {
                        null => ex,
                        AggregateException aggregateException => new AggregateException(aggregateException.InnerExceptions.Append(ex)),
                        _ => new AggregateException(exception, ex),
                    };

                    if (attempt < retryCount)
                        await Task
                            .Delay(retryDelay)
                            .ConfigureAwait(false);
                    else
                        throw exception;
                }
        });
    }

    /// <summary>
    ///     Retries awaiting the provided <see cref="Task{TResult}" /> when it faults, up to the specified number of retries.
    /// </summary>
    /// <typeparam name="T">The result type of the task.</typeparam>
    /// <param name="task">The task to await. If <c>null</c>, a completed task with <c>default(T)</c> is returned.</param>
    /// <param name="retryCount">The number of retries after the initial attempt. Must be zero or greater.</param>
    /// <param name="retryDelay">The delay between attempts. If <see cref="TimeSpan.Zero" />, a default of 1 second is used.</param>
    /// <returns>A task that completes with the task's result if a try succeeds, or throws after the final attempt.</returns>
    /// <remarks>
    ///     Exceptions from failed attempts are aggregated and thrown as an <see cref="AggregateException" /> after the final attempt.
    ///     Cancellation-related exceptions are rethrown immediately and are not retried.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="retryCount" /> is negative.</exception>
    /// <exception cref="OperationCanceledException">Rethrown if the operation is canceled.</exception>
    /// <exception cref="StackOverflowException">Rethrown without retry.</exception>
    public static Task<T> WithRetry<T>(this Task<T>? task, int retryCount = 5, TimeSpan retryDelay = default)
    {
        if (task is null)
            return Task.FromResult(default(T)!);

        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);

        if (retryDelay == TimeSpan.Zero)
            retryDelay = TimeSpan.FromSeconds(1);

        return Task.Run(async () =>
        {
            Exception? exception = null;

            for (var attempt = 0; attempt <= retryCount; attempt++)
                try
                {
                    return await task.ConfigureAwait(false);
                }
                catch (StackOverflowException)
                {
                    throw;
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    exception = exception switch
                    {
                        null => ex,
                        AggregateException aggregateException => new AggregateException(aggregateException.InnerExceptions.Append(ex)),
                        _ => new AggregateException(exception, ex),
                    };

                    if (attempt < retryCount)
                        await Task
                            .Delay(retryDelay)
                            .ConfigureAwait(false);
                    else
                        throw exception;
                }

            // Should never reach here
            // ReSharper disable once HeuristicUnreachableCode
            return default!;
        });
    }

    /// <summary>
    ///     Retries an asynchronous operation produced by <paramref name="taskFactory" /> when it faults.
    ///     A new task is created for each attempt by invoking the factory.
    /// </summary>
    /// <param name="taskFactory">A delegate that creates a new <see cref="Task" /> each attempt. Cannot be <c>null</c>.</param>
    /// <param name="retryCount">The number of retries after the initial attempt. Must be zero or greater.</param>
    /// <param name="retryDelay">The delay between attempts. If <see cref="TimeSpan.Zero" />, a default of 1 second is used.</param>
    /// <param name="cancellationToken">A token observed before each attempt and during delay between retries.</param>
    /// <returns>A task that completes when one attempt succeeds, or throws after the final attempt.</returns>
    /// <remarks>
    ///     Exceptions from failed attempts are aggregated and thrown as an <see cref="AggregateException" /> after the final attempt.
    ///     Cancellation-related exceptions are rethrown immediately and are not retried.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskFactory" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="retryCount" /> is negative.</exception>
    /// <exception cref="OperationCanceledException">Rethrown if the operation is canceled.</exception>
    /// <exception cref="StackOverflowException">Rethrown without retry.</exception>
    public static Task WithRetry(
        this Func<Task> taskFactory,
        int retryCount = 5,
        TimeSpan retryDelay = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactory);
        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);

        if (retryDelay == TimeSpan.Zero)
            retryDelay = TimeSpan.FromSeconds(1);

        return Task.Run(async () =>
            {
                Exception? exception = null;

                for (var attempt = 0; attempt <= retryCount; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        await taskFactory()
                            .ConfigureAwait(false);

                        return;
                    }
                    catch (StackOverflowException)
                    {
                        throw;
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        exception = exception switch
                        {
                            null => ex,
                            AggregateException aggregateException => new AggregateException(aggregateException.InnerExceptions.Append(ex)),
                            _ => new AggregateException(exception, ex),
                        };

                        if (attempt < retryCount)
                            await Task
                                .Delay(retryDelay, cancellationToken)
                                .ConfigureAwait(false);
                        else
                            throw exception;
                    }
                }
            },
            cancellationToken);
    }

    /// <summary>
    ///     Retries an asynchronous operation that produces a <typeparamref name="T" /> when it faults.
    ///     A new task is created for each attempt by invoking the factory.
    /// </summary>
    /// <typeparam name="T">The result type produced by the task.</typeparam>
    /// <param name="taskFactory">A delegate that creates a new <see cref="Task{TResult}" /> each attempt. Cannot be <c>null</c>.</param>
    /// <param name="retryCount">The number of retries after the initial attempt. Must be zero or greater.</param>
    /// <param name="retryDelay">The delay between attempts. If <see cref="TimeSpan.Zero" />, a default of 1 second is used.</param>
    /// <param name="cancellationToken">A token observed before each attempt and during delay between retries.</param>
    /// <returns>A task that completes with a result when one attempt succeeds, or throws after the final attempt.</returns>
    /// <remarks>
    ///     Exceptions from failed attempts are aggregated and thrown as an <see cref="AggregateException" /> after the final attempt.
    ///     Cancellation-related exceptions are rethrown immediately and are not retried.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="taskFactory" /> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="retryCount" /> is negative.</exception>
    /// <exception cref="OperationCanceledException">Rethrown if the operation is canceled.</exception>
    /// <exception cref="StackOverflowException">Rethrown without retry.</exception>
    public static Task<T> WithRetry<T>(
        this Func<Task<T>> taskFactory,
        int retryCount = 5,
        TimeSpan retryDelay = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskFactory);
        ArgumentOutOfRangeException.ThrowIfNegative(retryCount);

        if (retryDelay == TimeSpan.Zero)
            retryDelay = TimeSpan.FromSeconds(1);

        return Task.Run(async () =>
            {
                Exception? exception = null;

                for (var attempt = 0; attempt <= retryCount; attempt++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        return await taskFactory()
                            .ConfigureAwait(false);
                    }
                    catch (StackOverflowException)
                    {
                        throw;
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        exception = exception switch
                        {
                            null => ex,
                            AggregateException aggregateException => new AggregateException(aggregateException.InnerExceptions.Append(ex)),
                            _ => new AggregateException(exception, ex),
                        };

                        if (attempt < retryCount)
                            await Task
                                .Delay(retryDelay, cancellationToken)
                                .ConfigureAwait(false);
                        else
                            throw exception;
                    }
                }

                // Should never reach here
                // ReSharper disable once HeuristicUnreachableCode
                return default!;
            },
            cancellationToken);
    }
}
