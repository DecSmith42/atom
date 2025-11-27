namespace Atom;

[PublicAPI]
public sealed record MethodParam(string Type, string Name, string? DefaultValue = null, string? XmlDescription = null);

[PublicAPI]
public sealed record PropertyGetter(Action<CsharpWriter>? Body);

[PublicAPI]
public sealed record PropertySetter(Action<CsharpWriter>? Body, bool InitOnly);

[PublicAPI]
public sealed partial class CsharpWriter
{
    private readonly StringBuilder _sb = new();

    private int _indent;

    public IDisposable Indent
    {
        get
        {
            _indent++;

            return new ActionScope(() => _indent--);
        }
    }

    [GeneratedRegex("(?<!^)(?=[A-Z])")]
    private static partial Regex UppercaseRegex { get; }

    public IDisposable Block(string text = "")
    {
        if (text.Length > 0)
            WriteLine(text);

        WriteLine("{");

        _indent++;

        return new ActionScope(() =>
        {
            _indent--;
            WriteLine("}");
        });
    }

    public void Write(string text, bool noIndent = false) =>
        _sb.Append(noIndent
            ? text
            : $"{new(' ', _indent * 4)}{text}");

    public void WriteLine(string line, bool noIndent = false) =>
        _sb.AppendLine(noIndent
            ? line
            : $"{new(' ', _indent * 4)}{line}");

    public override string ToString() =>
        _sb.ToString();

    public void WriteProperty(
        string access,
        string type,
        string name,
        PropertyGetter? getter = null,
        PropertySetter? setter = null,
        string? defaultValue = null,
        string? xmlSummary = null)
    {
        if (xmlSummary is not null)
        {
            WriteLine("/// <summary>");

            foreach (var line in xmlSummary
                         .Replace("\r", "")
                         .Split('\n'))
                WriteLine($"///     {line}");

            WriteLine("/// </summary>");
        }

        WriteLine(access.Length > 0
            ? $"{access} {type} {name}"
            : $"{type} {name}");

        using (Block())
        {
            if (getter is not null)
            {
                if (getter.Body is not null)
                    getter.Body(this);
                else
                    WriteLine("get;");
            }

            if (setter is not null)
            {
                if (setter.Body is not null)
                    using (Block(setter.InitOnly
                               ? "init"
                               : "set"))
                        setter.Body(this);
                else
                    WriteLine(setter.InitOnly
                        ? "init;"
                        : "set;");
            }
        }

        if (defaultValue is not null)
            WriteLine($"= {defaultValue};");

        WriteLine(string.Empty);
    }

    public void WriteMethod(
        string access,
        string returnType,
        string name,
        IEnumerable<MethodParam?> parameters,
        Action<CsharpWriter>? body = null,
        bool expressionBodied = false,
        string? xmlSummary = null)
    {
        var validParameters = parameters
            .OfType<MethodParam>()
            .ToList();

        if (xmlSummary is not null)
        {
            WriteLine("/// <summary>");

            foreach (var line in xmlSummary
                         .Replace("\r", "")
                         .Split('\n'))
                WriteLine($"///     {line}");

            WriteLine("/// </summary>");

            foreach (var (paramName, paramDesc) in validParameters
                         .Select(x => (x.Name, x.XmlDescription))
                         .Where(x => x.XmlDescription is not null))
            {
                WriteLine($"/// <param name=\"{paramName}\">");

                foreach (var line in paramDesc!
                             .Replace("\r", "")
                             .Split('\n'))
                    WriteLine($"///     {line}");

                WriteLine("/// </param>");
            }
        }

        if (body is null)
        {
            if (validParameters.Count != 0)
            {
                WriteLine(access.Length > 0
                    ? $"{access} {returnType} {name}("
                    : $"{returnType} {name}(");

                using (Indent)
                {
                    for (var i = 0; i < validParameters.Count; i++)
                    {
                        var (paramType, paramName, defaultValue, _) = validParameters[i];

                        Write(defaultValue is { Length: > 0 }
                            ? $"{paramType} {paramName} = {defaultValue}"
                            : $"{paramType} {paramName}");

                        WriteLine(i < validParameters.Count - 1
                                ? ","
                                : string.Empty,
                            true);
                    }
                }

                WriteLine(");");
            }
            else
            {
                WriteLine($"{access} {returnType} {name}();");
            }

            WriteLine(string.Empty);

            return;
        }

        if (validParameters.Count != 0)
        {
            WriteLine($"{access} {returnType} {name}(");

            using (Indent)
            {
                for (var i = 0; i < validParameters.Count; i++)
                {
                    var (paramType, paramName, defaultValue, _) = validParameters[i];

                    Write(defaultValue is { Length: > 0 }
                        ? $"{paramType} {paramName} = {defaultValue}"
                        : $"{paramType} {paramName}");

                    WriteLine(i < validParameters.Count - 1
                            ? ","
                            : string.Empty,
                        true);
                }
            }

            Write(")");
        }
        else
        {
            Write($"{access} {returnType} {name}()");
        }

        if (expressionBodied)
        {
            WriteLine(" =>", true);

            using (Indent)
                body(this);
        }
        else
        {
            using (Block())
                body(this);
        }

        WriteLine(string.Empty);
    }

    public static string ToPascalCase(string text, bool lowerFirstWord = false)
    {
        // If all letters are uppercase, convert to lowercase
        if (text.All(x => char.IsUpper(x) || !char.IsLetter(x)))
            text = text.ToLowerInvariant();

        // Replace '|' with 'or'
        text = text.Replace("|", "or");

        // Add a space before each uppercase letter that is not at the start, and is not followed by another uppercase letter
        text = UppercaseRegex.Replace(text, " ");

        // Split by non-letter characters
        var parts = text.Split(['-', '_', ' ', ':'], StringSplitOptions.RemoveEmptyEntries);

        var pascalCase = string.Concat(parts.Select(part => char.ToUpperInvariant(part[0]) +
                                                            part[1..]
                                                                .ToLowerInvariant()));

        return lowerFirstWord
            ? char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..]
            : pascalCase;
    }
}
