namespace DecSm.Atom.Module.GithubWorkflows;

/// <summary>
///     Represents an abstract base class for all GitHub Actions expressions.
/// </summary>
/// <remarks>
///     This class provides a fluent API for constructing GitHub Actions expressions
///     that can be used in workflow definitions.
/// </remarks>
[PublicAPI]
public abstract record IGithubExpression
{
    /// <summary>
    ///     Writes the expression to its GitHub Actions string representation.
    /// </summary>
    /// <returns>The GitHub Actions expression string.</returns>
    protected abstract string Write();

    /// <inheritdoc />
    public override string ToString() =>
        Write();

    /// <summary>
    ///     Implicitly converts an <see cref="IGithubExpression" /> to its string representation.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>The GitHub Actions expression string.</returns>
    public static implicit operator string(IGithubExpression expression) =>
        expression.Write();

    /// <summary>
    ///     Implicitly converts a string literal to a <see cref="LiteralExpression" />.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>A <see cref="LiteralExpression" />.</returns>
    public static implicit operator IGithubExpression(string value) =>
        new LiteralExpression(value);

    /// <summary>
    ///     Wraps the current expression in parentheses for logical grouping.
    /// </summary>
    /// <returns>A <see cref="LogicalGroupingExpression" />.</returns>
    public IGithubExpression Grouped() =>
        new LogicalGroupingExpression(this);

    /// <summary>
    ///     Wraps the current expression in single quotes, treating it as a string literal.
    /// </summary>
    /// <returns>A <see cref="StringExpression" />.</returns>
    public IGithubExpression AsString() =>
        new StringExpression(this);

    /// <summary>
    ///     Accesses an element of an array or object by index.
    /// </summary>
    /// <returns>An <see cref="IndexedExpression" />.</returns>
    public IGithubExpression Indexed() =>
        new IndexedExpression(this);

    /// <summary>
    ///     Accesses a property of an object.
    /// </summary>
    /// <param name="sections">The property names or expressions representing the path to the property.</param>
    /// <returns>A <see cref="PropertyExpression" />.</returns>
    public IGithubExpression Property(params IGithubExpression[] sections) =>
        new PropertyExpression([this, .. sections]);

    /// <summary>
    ///     Applies the logical NOT operator to the expression.
    /// </summary>
    /// <returns>A <see cref="NotExpression" />.</returns>
    public IGithubExpression Not() =>
        new NotExpression(this);

    /// <summary>
    ///     Creates a less than comparison expression.
    /// </summary>
    /// <param name="right">The right-hand side of the comparison.</param>
    /// <returns>A <see cref="LessThanExpression" />.</returns>
    public IGithubExpression LessThan(IGithubExpression right) =>
        new LessThanExpression(this, right);

    /// <summary>
    ///     Creates a less than or equal to comparison expression.
    /// </summary>
    /// <param name="right">The right-hand side of the comparison.</param>
    /// <returns>A <see cref="LessThanOrEqualExpression" />.</returns>
    public IGithubExpression LessThanOrEqualTo(IGithubExpression right) =>
        new LessThanOrEqualExpression(this, right);

    /// <summary>
    ///     Creates a greater than comparison expression.
    /// </summary>
    /// <param name="right">The right-hand side of the comparison.</param>
    /// <returns>A <see cref="GreaterThanExpression" />.</returns>
    public IGithubExpression GreaterThan(IGithubExpression right) =>
        new GreaterThanExpression(this, right);

    /// <summary>
    ///     Creates a greater than or equal to comparison expression.
    /// </summary>
    /// <param name="right">The right-hand side of the comparison.</param>
    /// <returns>A <see cref="GreaterThanOrEqualExpression" />.</returns>
    public IGithubExpression GreaterThanOrEqualTo(IGithubExpression right) =>
        new GreaterThanOrEqualExpression(this, right);

    /// <summary>
    ///     Creates an equality comparison expression.
    /// </summary>
    /// <param name="right">The right-hand side of the comparison.</param>
    /// <returns>An <see cref="EqualExpression" />.</returns>
    public IGithubExpression EqualTo(IGithubExpression right) =>
        new EqualExpression(this, right);

    /// <summary>
    ///     Creates a not equal to comparison expression.
    /// </summary>
    /// <param name="right">The right-hand side of the comparison.</param>
    /// <returns>A <see cref="NotEqualExpression" />.</returns>
    public IGithubExpression NotEqualTo(IGithubExpression right) =>
        new NotEqualExpression(this, right);

    /// <summary>
    ///     Creates a logical AND expression.
    /// </summary>
    /// <param name="right">The right-hand side of the AND operation.</param>
    /// <returns>An <see cref="AndExpression" />.</returns>
    public IGithubExpression And(IGithubExpression right) =>
        new AndExpression(this, right);

    /// <summary>
    ///     Creates a logical OR expression.
    /// </summary>
    /// <param name="right">The right-hand side of the OR operation.</param>
    /// <returns>An <see cref="OrExpression" />.</returns>
    public IGithubExpression Or(IGithubExpression right) =>
        new OrExpression(this, right);

    /// <summary>
    ///     Creates a `contains()` function expression.
    /// </summary>
    /// <param name="item">The item to search for.</param>
    /// <returns>A <see cref="ContainsExpression" />.</returns>
    public IGithubExpression Contains(IGithubExpression item) =>
        new ContainsExpression(this, item);

    /// <summary>
    ///     Creates a `startsWith()` function expression.
    /// </summary>
    /// <param name="searchValue">The value to check if the string starts with.</param>
    /// <returns>A <see cref="StartsWithExpression" />.</returns>
    public IGithubExpression StartsWith(IGithubExpression searchValue) =>
        new StartsWithExpression(this, searchValue);

    /// <summary>
    ///     Creates an `endsWith()` function expression.
    /// </summary>
    /// <param name="searchValue">The value to check if the string ends with.</param>
    /// <returns>An <see cref="EndsWithExpression" />.</returns>
    public IGithubExpression EndsWith(IGithubExpression searchValue) =>
        new EndsWithExpression(this, searchValue);

    /// <summary>
    ///     Creates a `format()` function expression.
    /// </summary>
    /// <param name="replaceValues">The values to insert into the format string.</param>
    /// <returns>A <see cref="FormatExpression" />.</returns>
    public IGithubExpression Format(params IGithubExpression[] replaceValues) =>
        new FormatExpression(this, replaceValues);

    /// <summary>
    ///     Creates a `join()` function expression.
    /// </summary>
    /// <param name="optionalSeparator">An optional separator to use when joining array elements.</param>
    /// <returns>A <see cref="JoinExpression" />.</returns>
    public IGithubExpression Join(IGithubExpression? optionalSeparator = null) =>
        new JoinExpression(this, optionalSeparator);

    /// <summary>
    ///     Creates a `toJSON()` function expression.
    /// </summary>
    /// <returns>A <see cref="ToJsonExpression" />.</returns>
    public IGithubExpression ToJson() =>
        new ToJsonExpression(this);

    /// <summary>
    ///     Creates a `fromJSON()` function expression.
    /// </summary>
    /// <returns>A <see cref="FromJsonExpression" />.</returns>
    public IGithubExpression FromJson() =>
        new FromJsonExpression(this);
}

// Literals

/// <summary>
///     Represents a literal string value in a GitHub Actions expression.
/// </summary>
/// <param name="Value">The literal string value.</param>
[PublicAPI]
public sealed record LiteralExpression(string Value) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        Value;

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a boolean literal value in a GitHub Actions expression.
/// </summary>
/// <param name="Value">The boolean value.</param>
[PublicAPI]
public sealed record BoolExpression(bool Value) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        Value
            ? "true"
            : "false";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a null literal value in a GitHub Actions expression.
/// </summary>
[PublicAPI]
public sealed record NullExpression : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        "null";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a number literal value in a GitHub Actions expression.
/// </summary>
/// <param name="Value">The numeric value.</param>
[PublicAPI]
public sealed record NumberExpression(double Value) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        Value switch
        {
            double.NaN => "0",
            double.PositiveInfinity => "2147483647",
            double.NegativeInfinity => "-2147483648",
            _ => Value.ToString(CultureInfo.InvariantCulture),
        };

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a string literal value in a GitHub Actions expression, ensuring proper escaping.
/// </summary>
/// <param name="Value">The string value.</param>
[PublicAPI]
public sealed record StringExpression(string Value) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        Value switch
        {
            null => "null",
            _ => $"'{Value.Replace("'", "''")}'", // Escape single quotes by doubling them
        };

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

// Operators

/// <summary>
///     Represents a logical grouping of an expression using parentheses.
/// </summary>
/// <param name="Contents">The expression to group.</param>
[PublicAPI]
public sealed record LogicalGroupingExpression(IGithubExpression Contents) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"({Contents})";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents an indexed access expression (e.g., `array[index]` or `object['key']`).
/// </summary>
/// <param name="Index">The expression representing the index or key.</param>
[PublicAPI]
public sealed record IndexedExpression(IGithubExpression Index) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"[{Index}]";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a property access expression (e.g., `object.property` or `object.nested.property`).
/// </summary>
/// <param name="Sections">The expressions representing the property path segments.</param>
[PublicAPI]
public sealed record PropertyExpression(params IGithubExpression[] Sections) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        Sections.Length switch
        {
            0 => throw new ArgumentException("PropertyExpression must have at least one section."),
            1 => Sections[0]
                .ToString(),
            _ => string.Join(".", Sections.Select(section => section.ToString())),
        };

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a logical NOT operator expression.
/// </summary>
/// <param name="Contents">The expression to negate.</param>
[PublicAPI]
public sealed record NotExpression(IGithubExpression Contents) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"!{Contents}";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a less than (`&lt;`) comparison expression.
/// </summary>
/// <param name="Left">The left-hand side of the comparison.</param>
/// <param name="Right">The right-hand side of the comparison.</param>
[PublicAPI]
public sealed record LessThanExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"{Left} < {Right}";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a less than or equal to (`&lt;=`) comparison expression.
/// </summary>
/// <param name="Left">The left-hand side of the comparison.</param>
/// <param name="Right">The right-hand side of the comparison.</param>
[PublicAPI]
public sealed record LessThanOrEqualExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"{Left} <= {Right}";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a greater than (`>`) comparison expression.
/// </summary>
/// <param name="Left">The left-hand side of the comparison.</param>
/// <param name="Right">The right-hand side of the comparison.</param>
[PublicAPI]
public sealed record GreaterThanExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"{Left} > {Right}";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a greater than or equal to (`>=`) comparison expression.
/// </summary>
/// <param name="Left">The left-hand side of the comparison.</param>
/// <param name="Right">The right-hand side of the comparison.</param>
[PublicAPI]
public sealed record GreaterThanOrEqualExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"{Left} >= {Right}";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents an equality (`==`) comparison expression.
/// </summary>
/// <param name="Left">The left-hand side of the comparison.</param>
/// <param name="Right">The right-hand side of the comparison.</param>
[PublicAPI]
public sealed record EqualExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"{Left} == {Right}";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a not equal to (`!=`) comparison expression.
/// </summary>
/// <param name="Left">The left-hand side of the comparison.</param>
/// <param name="Right">The right-hand side of the comparison.</param>
[PublicAPI]
public sealed record NotEqualExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"{Left} != {Right}";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a logical AND (`&amp;&amp;`) expression.
/// </summary>
/// <param name="Left">The left-hand side of the AND operation.</param>
/// <param name="Right">The right-hand side of the AND operation.</param>
[PublicAPI]
public sealed record AndExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"{Left} && {Right}";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents a logical OR (`||`) expression.
/// </summary>
/// <param name="Left">The left-hand side of the OR operation.</param>
/// <param name="Right">The right-hand side of the OR operation.</param>
[PublicAPI]
public sealed record OrExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"{Left} || {Right}";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

// Functions

/// <summary>
///     Represents the `contains()` function in GitHub Actions expressions.
/// </summary>
/// <param name="Search">The collection or string to search within.</param>
/// <param name="Item">The item or substring to search for.</param>
[PublicAPI]
public sealed record ContainsExpression(IGithubExpression Search, IGithubExpression Item) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"contains({Search}, {Item})";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents the `startsWith()` function in GitHub Actions expressions.
/// </summary>
/// <param name="SearchString">The string to check.</param>
/// <param name="SearchValue">The value to check if the string starts with.</param>
[PublicAPI]
public sealed record StartsWithExpression(IGithubExpression SearchString, IGithubExpression SearchValue)
    : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"startsWith({SearchString}, {SearchValue})";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents the `endsWith()` function in GitHub Actions expressions.
/// </summary>
/// <param name="SearchString">The string to check.</param>
/// <param name="SearchValue">The value to check if the string ends with.</param>
[PublicAPI]
public sealed record EndsWithExpression(IGithubExpression SearchString, IGithubExpression SearchValue)
    : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"endsWith({SearchString}, {SearchValue})";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents the `format()` function in GitHub Actions expressions.
/// </summary>
/// <param name="String">The format string.</param>
/// <param name="ReplaceValues">The values to insert into the format string.</param>
[PublicAPI]
public sealed record FormatExpression(IGithubExpression String, params IGithubExpression[] ReplaceValues)
    : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        ReplaceValues.Length switch
        {
            0 => throw new ArgumentException("FormatExpression must have at least one replace value."),
            1 => $"format({String}, {ReplaceValues[0]})",
            _ =>
                $"format({String}, {string.Join(", ", ReplaceValues.Select(replaceValue => replaceValue.ToString()))})",
        };

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents the `join()` function in GitHub Actions expressions.
/// </summary>
/// <param name="Array">The array to join.</param>
/// <param name="OptionalSeparator">An optional separator to use when joining array elements.</param>
[PublicAPI]
public sealed record JoinExpression(IGithubExpression Array, IGithubExpression? OptionalSeparator = null)
    : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        OptionalSeparator is null
            ? $"join({Array})"
            : $"join({Array}, {OptionalSeparator})";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents the `toJSON()` function in GitHub Actions expressions.
/// </summary>
/// <param name="Value">The value to convert to a JSON string.</param>
[PublicAPI]
public sealed record ToJsonExpression(IGithubExpression Value) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"toJSON({Value})";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents the `fromJSON()` function in GitHub Actions expressions.
/// </summary>
/// <param name="Value">The JSON string to convert to an object.</param>
[PublicAPI]
public sealed record FromJsonExpression(IGithubExpression Value) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        $"fromJSON({Value})";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents the `hashFiles()` function in GitHub Actions expressions.
/// </summary>
/// <param name="Paths">One or more file paths or glob patterns to hash.</param>
[PublicAPI]
public sealed record HashFilesExpression(params IGithubExpression[] Paths) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        Paths.Length switch
        {
            0 => throw new ArgumentException("HashFilesExpression must have at least one path."),
            1 => $"hashFiles({Paths[0]})",
            _ => $"hashFiles({string.Join(", ", Paths.Select(path => path.ToString()))})",
        };

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

// Status Check Functions

/// <summary>
///     Represents the `success()` status check function in GitHub Actions expressions.
/// </summary>
/// <remarks>
///     Returns `true` when all previous steps have succeeded.
/// </remarks>
[PublicAPI]
public sealed record SuccessExpression : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        "success()";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents the `always()` status check function in GitHub Actions expressions.
/// </summary>
/// <remarks>
///     Returns `true` even when previous steps have failed or been cancelled.
/// </remarks>
[PublicAPI]
public sealed record AlwaysExpression : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        "always()";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents the `cancelled()` status check function in GitHub Actions expressions.
/// </summary>
/// <remarks>
///     Returns `true` if the workflow was cancelled.
/// </remarks>
[PublicAPI]
public sealed record CancelledExpression : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        "cancelled()";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents the `failure()` status check function in GitHub Actions expressions.
/// </summary>
/// <remarks>
///     Returns `true` when any previous step has failed.
/// </remarks>
[PublicAPI]
public sealed record FailureExpression : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        "failure()";

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

// Consumed Expressions
/// <summary>
///     Represents an expression to consume a variable from a previous job's output.
/// </summary>
/// <param name="JobName">The name of the job that produced the output.</param>
/// <param name="VariableName">The name of the output variable.</param>
[PublicAPI]
public sealed record ConsumedVariableExpression(IGithubExpression JobName, IGithubExpression VariableName)
    : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        new PropertyExpression("needs", JobName, "outputs", VariableName);

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}

/// <summary>
///     Represents an expression to consume the result of a previous job.
/// </summary>
/// <param name="JobName">The name of the job whose result is to be consumed.</param>
[PublicAPI]
public sealed record ConsumedResultExpression(IGithubExpression JobName) : IGithubExpression
{
    /// <inheritdoc />
    protected override string Write() =>
        new PropertyExpression("needs", JobName, "result");

    /// <inheritdoc />
    public override string ToString() =>
        Write();
}
