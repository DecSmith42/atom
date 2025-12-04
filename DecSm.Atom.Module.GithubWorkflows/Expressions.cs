namespace DecSm.Atom.Module.GithubWorkflows;

[PublicAPI]
public abstract record IGithubExpression
{
    protected abstract string Write();

    public override string ToString() =>
        Write();

    public static implicit operator string(IGithubExpression expression) =>
        expression.Write();

    public static implicit operator IGithubExpression(string value) =>
        new LiteralExpression(value);

    public IGithubExpression Grouped() =>
        new LogicalGroupingExpression(this);

    public IGithubExpression AsString() =>
        new StringExpression(this);

    public IGithubExpression Indexed() =>
        new IndexedExpression(this);

    public IGithubExpression Property(params IGithubExpression[] sections) =>
        new PropertyExpression([this, .. sections]);

    public IGithubExpression Not() =>
        new NotExpression(this);

    public IGithubExpression LessThan(IGithubExpression right) =>
        new LessThanExpression(this, right);

    public IGithubExpression LessThanOrEqualTo(IGithubExpression right) =>
        new LessThanOrEqualExpression(this, right);

    public IGithubExpression GreaterThan(IGithubExpression right) =>
        new GreaterThanExpression(this, right);

    public IGithubExpression GreaterThanOrEqualTo(IGithubExpression right) =>
        new GreaterThanOrEqualExpression(this, right);

    public IGithubExpression EqualTo(IGithubExpression right) =>
        new EqualExpression(this, right);

    public IGithubExpression NotEqualTo(IGithubExpression right) =>
        new NotEqualExpression(this, right);

    public IGithubExpression And(IGithubExpression right) =>
        new AndExpression(this, right);

    public IGithubExpression Or(IGithubExpression right) =>
        new OrExpression(this, right);

    public IGithubExpression Contains(IGithubExpression item) =>
        new ContainsExpression(this, item);

    public IGithubExpression StartsWith(IGithubExpression searchValue) =>
        new StartsWithExpression(this, searchValue);

    public IGithubExpression EndsWith(IGithubExpression searchValue) =>
        new EndsWithExpression(this, searchValue);

    public IGithubExpression Format(params IGithubExpression[] replaceValues) =>
        new FormatExpression(this, replaceValues);

    public IGithubExpression Join(IGithubExpression? optionalSeparator = null) =>
        new JoinExpression(this, optionalSeparator);

    public IGithubExpression ToJson() =>
        new ToJsonExpression(this);

    public IGithubExpression FromJson() =>
        new FromJsonExpression(this);
}

// Literals

[PublicAPI]
public sealed record LiteralExpression(string Value) : IGithubExpression
{
    protected override string Write() =>
        Value;

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record BoolExpression(bool Value) : IGithubExpression
{
    protected override string Write() =>
        Value
            ? "true"
            : "false";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record NullExpression : IGithubExpression
{
    protected override string Write() =>
        "null";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record NumberExpression(double Value) : IGithubExpression
{
    protected override string Write() =>
        Value switch
        {
            double.NaN => "0",
            double.PositiveInfinity => "2147483647",
            double.NegativeInfinity => "-2147483648",
            _ => Value.ToString(CultureInfo.InvariantCulture),
        };

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record StringExpression(string Value) : IGithubExpression
{
    protected override string Write() =>
        Value switch
        {
            null => "null",
            _ => $"'{Value.Replace("'", "''")}'", // Escape single quotes by doubling them
        };

    public override string ToString() =>
        Write();
}

// Operators

[PublicAPI]
public sealed record LogicalGroupingExpression(IGithubExpression Contents) : IGithubExpression
{
    protected override string Write() =>
        $"({Contents})";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record IndexedExpression(IGithubExpression Index) : IGithubExpression
{
    protected override string Write() =>
        $"[{Index}]";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record PropertyExpression(params IGithubExpression[] Sections) : IGithubExpression
{
    protected override string Write() =>
        Sections.Length switch
        {
            0 => throw new ArgumentException("PropertyExpression must have at least one section."),
            1 => Sections[0]
                .ToString(),
            _ => string.Join(".", Sections.Select(section => section.ToString())),
        };

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record NotExpression(IGithubExpression Contents) : IGithubExpression
{
    protected override string Write() =>
        $"!{Contents}";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record LessThanExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    protected override string Write() =>
        $"{Left} < {Right}";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record LessThanOrEqualExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    protected override string Write() =>
        $"{Left} <= {Right}";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record GreaterThanExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    protected override string Write() =>
        $"{Left} > {Right}";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record GreaterThanOrEqualExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    protected override string Write() =>
        $"{Left} >= {Right}";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record EqualExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    protected override string Write() =>
        $"{Left} == {Right}";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record NotEqualExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    protected override string Write() =>
        $"{Left} != {Right}";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record AndExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    protected override string Write() =>
        $"{Left} && {Right}";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record OrExpression(IGithubExpression Left, IGithubExpression Right) : IGithubExpression
{
    protected override string Write() =>
        $"{Left} || {Right}";

    public override string ToString() =>
        Write();
}

// Functions

[PublicAPI]
public sealed record ContainsExpression(IGithubExpression Search, IGithubExpression Item) : IGithubExpression
{
    protected override string Write() =>
        $"contains({Search}, {Item})";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record StartsWithExpression(IGithubExpression SearchString, IGithubExpression SearchValue)
    : IGithubExpression
{
    protected override string Write() =>
        $"startsWith({SearchString}, {SearchValue})";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record EndsWithExpression(IGithubExpression SearchString, IGithubExpression SearchValue)
    : IGithubExpression
{
    protected override string Write() =>
        $"endsWith({SearchString}, {SearchValue})";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record FormatExpression(IGithubExpression String, params IGithubExpression[] ReplaceValues)
    : IGithubExpression
{
    protected override string Write() =>
        ReplaceValues.Length switch
        {
            0 => throw new ArgumentException("FormatExpression must have at least one replace value."),
            1 => $"format({String}, {ReplaceValues[0]})",
            _ =>
                $"format({String}, {string.Join(", ", ReplaceValues.Select(replaceValue => replaceValue.ToString()))})",
        };

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record JoinExpression(IGithubExpression Array, IGithubExpression? OptionalSeparator = null)
    : IGithubExpression
{
    protected override string Write() =>
        OptionalSeparator is null
            ? $"join({Array})"
            : $"join({Array}, {OptionalSeparator})";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record ToJsonExpression(IGithubExpression Value) : IGithubExpression
{
    protected override string Write() =>
        $"toJSON({Value})";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record FromJsonExpression(IGithubExpression Value) : IGithubExpression
{
    protected override string Write() =>
        $"fromJSON({Value})";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record HashFilesExpression(params IGithubExpression[] Paths) : IGithubExpression
{
    protected override string Write() =>
        Paths.Length switch
        {
            0 => throw new ArgumentException("HashFilesExpression must have at least one path."),
            1 => $"hashFiles({Paths[0]})",
            _ => $"hashFiles({string.Join(", ", Paths.Select(path => path.ToString()))})",
        };

    public override string ToString() =>
        Write();
}

// Status Check Functions

[PublicAPI]
public sealed record SuccessExpression : IGithubExpression
{
    protected override string Write() =>
        "success()";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record AlwaysExpression : IGithubExpression
{
    protected override string Write() =>
        "always()";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record CancelledExpression : IGithubExpression
{
    protected override string Write() =>
        "cancelled()";

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record FailureExpression : IGithubExpression
{
    protected override string Write() =>
        "failure()";

    public override string ToString() =>
        Write();
}

// Consumed Expressions
[PublicAPI]
public sealed record ConsumedVariableExpression(IGithubExpression JobName, IGithubExpression VariableName)
    : IGithubExpression
{
    protected override string Write() =>
        new PropertyExpression("needs", JobName, "outputs", VariableName);

    public override string ToString() =>
        Write();
}

[PublicAPI]
public sealed record ConsumedResultExpression(IGithubExpression JobName) : IGithubExpression
{
    protected override string Write() =>
        new PropertyExpression("needs", JobName, "result");

    public override string ToString() =>
        Write();
}
