using System.Globalization;

namespace DecSm.Atom.Module.GithubWorkflows.Tests;

// ## About expressions
//
// You can use expressions to programmatically set environment variables in workflow files and access contexts. An expression can be any combination of literal values, references to a context, or functions. You can combine literals, context references, and functions using operators. For more information about contexts, see [AUTOTITLE](/actions/learn-github-actions/contexts).
//
// Expressions are commonly used with the conditional `if` keyword in a workflow file to determine whether a step should run. When an `if` conditional is `true`, the step will run.
//
// {% data reusables.actions.expressions-syntax-evaluation %}
//
// {% raw %}
// `${{ <expression> }}`
// {% endraw %}
//
// > [!NOTE]
// > The exception to this rule is when you are using expressions in an `if` clause, where, optionally, you can usually omit {% raw %}`${{`{% endraw %} and {% raw %}`}}`{% endraw %}. For more information about `if` conditionals, see [AUTOTITLE](/actions/using-workflows/workflow-syntax-for-github-actions#jobsjob_idif).
//
// {% data reusables.actions.context-injection-warning %}
//
// ### Example setting an environment variable
//
// {% raw %}
//
// ```yaml
// env:
//   MY_ENV_VAR: ${{ <expression> }}
// ```
//
// {% endraw %}
//
// ## Literals
//
// As part of an expression, you can use `boolean`, `null`, `number`, or `string` data types.
//
// | Data type | Literal value |
// |-----------|---------------|
// | `boolean` | `true` or `false` |
// | `null`    | `null` |
// | `number`  | Any number format supported by JSON. |
// | `string`  | You don't need to enclose strings in `{% raw %}${{{% endraw %}` and `{% raw %}}}{% endraw %}`. However, if you do, you must use single quotes (`'`) around the string. To use a literal single quote, escape the literal single quote using an additional single quote (`''`). Wrapping with double quotes (`"`) will throw an error. |
//
// Note that in conditionals, falsy values (`false`, `0`, `-0`, `""`, `''`, `null`) are coerced to `false` and truthy (`true` and other non-falsy values) are coerced to `true`.
//
// ### Example of literals
//
// {% raw %}
//
// ```yaml
// env:
//   myNull: ${{ null }}
//   myBoolean: ${{ false }}
//   myIntegerNumber: ${{ 711 }}
//   myFloatNumber: ${{ -9.2 }}
//   myHexNumber: ${{ 0xff }}
//   myExponentialNumber: ${{ -2.99e-2 }}
//   myString: Mona the Octocat
//   myStringInBraces: ${{ 'It''s open source!' }}
// ```
//
// {% endraw %}
//
// ## Operators
//
// | Operator    | Description |
// | ---         | ---         |
// | `( )`       | Logical grouping |
// | `[ ]`       | Index |
// | `.`         | Property de-reference |
// | `!`         | Not |
// | `<`         | Less than |
// | `<=`        | Less than or equal |
// | `>`         | Greater than |
// | `>=`        | Greater than or equal |
// | `==`        | Equal |
// | `!=`        | Not equal |
// | `&&`        | And |
// |  <code>\|\|</code> | Or |
//
//   > [!NOTE]
//   > * {% data variables.product.company_short %} ignores case when comparing strings.
//   > * `steps.<step_id>.outputs.<output_name>` evaluates as a string. {% data reusables.actions.expressions-syntax-evaluation %} For more information, see [AUTOTITLE](/actions/learn-github-actions/contexts#steps-context).
//   > * For numerical comparison, the `fromJSON()` function can be used to convert a string to a number. For more information on the `fromJSON()` function, see [fromJSON](#fromjson).
//
// {% data variables.product.prodname_dotcom %} performs loose equality comparisons.
//
// * If the types do not match, {% data variables.product.prodname_dotcom %} coerces the type to a number. {% data variables.product.prodname_dotcom %} casts data types to a number using these conversions:
//
//   | Type    | Result |
//   | ---     | ---    |
//   | Null    | `0` |
//   | Boolean | `true` returns `1` <br /> `false` returns `0` |
//   | String  | Parsed from any legal JSON number format, otherwise `NaN`. <br /> Note: empty string returns `0`. |
//   | Array   | `NaN` |
//   | Object  | `NaN` |
// * When `NaN` is one of the operands of any relational comparison (`>`, `<`, `>=`, `<=`), the result is always `false`. For more information, see the [NaN Mozilla docs](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/NaN).
// * {% data variables.product.prodname_dotcom %} ignores case when comparing strings.
// * Objects and arrays are only considered equal when they are the same instance.
//
// {% data variables.product.prodname_dotcom %} provides a way to create conditional logic in expressions using binary logical operators (`&&` and `||`). This pattern can be used to achieve similar functionality to the ternary operator (`?:`) found in many programming languages, while actually using only binary operators.
//
// ## Functions
//
// {% data variables.product.prodname_dotcom %} offers a set of built-in functions that you can use in expressions. Some functions cast values to a string to perform comparisons. {% data variables.product.prodname_dotcom %} casts data types to a string using these conversions:
//
// | Type    | Result |
// | ---     | ---    |
// | Null    | `''` |
// | Boolean | `'true'` or `'false'` |
// | Number  | Decimal format, exponential for large numbers |
// | Array   | Arrays are not converted to a string |
// | Object  | Objects are not converted to a string |
//
// ### contains
//
// `contains( search, item )`
//
// Returns `true` if `search` contains `item`. If `search` is an array, this function returns `true` if the `item` is an element in the array. If `search` is a string, this function returns `true` if the `item` is a substring of `search`. This function is not case sensitive. Casts values to a string.
//
// #### Example using a string
//
// `contains('Hello world', 'llo')` returns `true`.
//
// #### Example using an object filter
//
// `contains(github.event.issue.labels.*.name, 'bug')` returns `true` if the issue related to the event has a label "bug".
//
// For more information, see [Object filters](#object-filters).
//
// #### Example matching an array of strings
//
// Instead of writing `github.event_name == "push" || github.event_name == "pull_request"`, you can use `contains()` with `fromJSON()` to check if an array of strings contains an `item`.
//
// For example, `contains(fromJSON('["push", "pull_request"]'), github.event_name)` returns `true` if `github.event_name` is "push" or "pull_request".
//
// ### startsWith
//
// `startsWith( searchString, searchValue )`
//
// Returns `true` when `searchString` starts with `searchValue`. This function is not case sensitive. Casts values to a string.
//
// #### Example of `startsWith`
//
// `startsWith('Hello world', 'He')` returns `true`.
//
// ### endsWith
//
// `endsWith( searchString, searchValue )`
//
// Returns `true` if `searchString` ends with `searchValue`. This function is not case sensitive. Casts values to a string.
//
// #### Example of `endsWith`
//
// `endsWith('Hello world', 'ld')` returns `true`.
//
// ### format
//
// `format( string, replaceValue0, replaceValue1, ..., replaceValueN)`
//
// Replaces values in the `string`, with the variable `replaceValueN`. Variables in the `string` are specified using the `{N}` syntax, where `N` is an integer. You must specify at least one `replaceValue` and `string`. There is no maximum for the number of variables (`replaceValueN`) you can use. Escape curly braces using double braces.
//
// #### Example of `format`
//
// {% raw %}
//
// ```javascript
// format('Hello {0} {1} {2}', 'Mona', 'the', 'Octocat')
// ```
//
// {% endraw %}
//
// Returns 'Hello Mona the Octocat'.
//
// #### Example escaping braces
//
// {% raw %}
//
// ```javascript
// format('{{Hello {0} {1} {2}!}}', 'Mona', 'the', 'Octocat')
// ```
//
// {% endraw %}
//
// Returns '{Hello Mona the Octocat!}'.
//
// ### join
//
// `join( array, optionalSeparator )`
//
// The value for `array` can be an array or a string. All values in `array` are concatenated into a string. If you provide `optionalSeparator`, it is inserted between the concatenated values. Otherwise, the default separator `,` is used. Casts values to a string.
//
// #### Example of `join`
//
// `join(github.event.issue.labels.*.name, ', ')` may return 'bug, help wanted'
//
// ### toJSON
//
// `toJSON(value)`
//
// Returns a pretty-print JSON representation of `value`. You can use this function to debug the information provided in contexts.
//
// #### Example of `toJSON`
//
// `toJSON(job)` might return `{ "status": "success" }`
//
// ### fromJSON
//
// `fromJSON(value)`
//
// Returns a JSON object or JSON data type for `value`. You can use this function to provide a JSON object as an evaluated expression or to convert any data type that can be represented in JSON or JavaScript, such as strings, booleans, null values, arrays, and objects.
//
// #### Example returning a JSON object
//
// This workflow sets a JSON matrix in one job, and passes it to the next job using an output and `fromJSON`.
//
// {% raw %}
//
// ```yaml copy
// name: build
// on: push
// jobs:
//   job1:
//     runs-on: ubuntu-latest
//     outputs:
//       matrix: ${{ steps.set-matrix.outputs.matrix }}
//     steps:
//       - id: set-matrix
//         run: echo "matrix={\"include\":[{\"project\":\"foo\",\"config\":\"Debug\"},{\"project\":\"bar\",\"config\":\"Release\"}]}" >> $GITHUB_OUTPUT
//   job2:
//     needs: job1
//     runs-on: ubuntu-latest
//     strategy:
//       matrix: ${{ fromJSON(needs.job1.outputs.matrix) }}
//     steps:
//       - run: echo "Matrix - Project ${{ matrix.project }}, Config ${{ matrix.config }}"
// ```
//
// {% endraw %}
//
// #### Example returning a JSON data type
//
// This workflow uses `fromJSON` to convert environment variables from a string to a Boolean or integer.
//
// ```yaml copy
// name: print
// on: push
// env:
//   continue: true
//   time: 3
// jobs:
//   job1:
//     runs-on: ubuntu-latest
//     steps:
//       - continue-on-error: {% raw %}${{ fromJSON(env.continue) }}{% endraw %}
//         timeout-minutes: {% raw %}${{ fromJSON(env.time) }}{% endraw %}
//         run: echo ...
// ```
//
// The workflow uses the `fromJSON()` function to convert the environment variable `continue` from a string to a boolean, allowing it to determine whether to continue-on-error or not. Similarly, it converts the `time` environment variable from a string to an integer, setting the timeout for the job in minutes.
//
// ### hashFiles
//
// `hashFiles(path)`
//
// Returns a single hash for the set of files that matches the `path` pattern. You can provide a single `path` pattern or multiple `path` patterns separated by commas. The `path` is relative to the `GITHUB_WORKSPACE` directory and can only include files inside of the `GITHUB_WORKSPACE`. This function calculates an individual SHA-256 hash for each matched file, and then uses those hashes to calculate a final SHA-256 hash for the set of files. If the `path` pattern does not match any files, this returns an empty string. For more information about SHA-256, see [SHA-2](https://en.wikipedia.org/wiki/SHA-2).
//
// You can use pattern matching characters to match file names. Pattern matching for `hashFiles` follows glob pattern matching and is case-insensitive on Windows. For more information about supported pattern matching characters, see the [Patterns](https://www.npmjs.com/package/@actions/glob#patterns) section in the `@actions/glob` documentation.
//
// #### Examples with a single pattern
//
// Matches any `package-lock.json` file in the repository.
//
// `hashFiles('**/package-lock.json')`
//
// Matches all `.js` files in the `src` directory at root level, but ignores any subdirectories of `src`.
//
// `hashFiles('/src/*.js')`
//
// Matches all `.rb` files in the `lib` directory at root level, including any subdirectories of `lib`.
//
// `hashFiles('/lib/**/*.rb')`
//
// #### Examples with multiple patterns
//
// Creates a hash for any `package-lock.json` and `Gemfile.lock` files in the repository.
//
// `hashFiles('**/package-lock.json', '**/Gemfile.lock')`
//
// Creates a hash for all `.rb` files in the `lib` directory at root level, including any subdirectories of `lib`, but excluding `.rb` files in the `foo` subdirectory.
//
// `hashFiles('/lib/**/*.rb', '!/lib/foo/*.rb')`
//
// ## Status check functions
//
// You can use the following status check functions as expressions in `if` conditionals. A default status check of `success()` is applied unless you include one of these functions. For more information about `if` conditionals, see [AUTOTITLE](/actions/using-workflows/workflow-syntax-for-github-actions#jobsjob_idif) and [AUTOTITLE](/actions/creating-actions/metadata-syntax-for-github-actions#runsstepsif).
//
// ### success
//
// Returns `true` when all previous steps have succeeded.
//
// #### Example of `success`
//
// ```yaml
// steps:
//   ...
//   - name: The job has succeeded
//     if: {% raw %}${{ success() }}{% endraw %}
// ```
//
// ### always
//
// Causes the step to always execute, and returns `true`, even when canceled. The `always` expression is best used at the step level or on tasks that you expect to run even when a job is canceled. For example, you can use `always` to send logs even when a job is canceled.
//
// > [!WARNING]
// > Avoid using `always` for any task that could suffer from a critical failure, for example: getting sources, otherwise the workflow may hang until it times out. If you want to run a job or step regardless of its success or failure, use the recommended alternative: `if: {% raw %}${{ !cancelled() }}{% endraw %}`
//
// #### Example of `always`
//
// ```yaml
// if: {% raw %}${{ always() }}{% endraw %}
// ```
//
// ### cancelled
//
// Returns `true` if the workflow was canceled.
//
// #### Example of `cancelled`
//
// ```yaml
// if: {% raw %}${{ cancelled() }}{% endraw %}
// ```
//
// ### failure
//
// Returns `true` when any previous step of a job fails. If you have a chain of dependent jobs, `failure()` returns `true` if any ancestor job fails.
//
// #### Example of `failure`
//
// ```yaml
// steps:
//   ...
//   - name: The job has failed
//     if: {% raw %}${{ failure() }}{% endraw %}
// ```
//
// #### failure with conditions
//
// You can include extra conditions for a step to run after a failure, but you must still include `failure()` to override the default status check of `success()` that is automatically applied to `if` conditions that don't contain a status check function.
//
// ##### Example of `failure` with conditions
//
// ```yaml
// steps:
//   ...
//   - name: Failing step
//     id: demo
//     run: exit 1
//   - name: The demo step has failed
//     if: {% raw %}${{ failure() && steps.demo.conclusion == 'failure' }}{% endraw %}
// ```

[TestFixture]
public class ExpressionsTests
{
    [Test]
    public void ImplicitConversion_String_To_IGithubExpression()
    {
        // Arrange
        const string value = "test-value";

        // Act
        IGithubExpression expression = value;

        // Assert
        expression.ShouldBeOfType<LiteralExpression>();

        expression
            .ToString()
            .ShouldBe(value);
    }

    [Test]
    public void ImplicitConversion_IGithubExpression_To_String()
    {
        // Arrange
        const string value = "test-value";
        const string expectedValue = "'test-value'";
        IGithubExpression expression = new StringExpression(value);

        // Act
        string result = expression;

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Test]
    public void ToString_Returns_Write_Result()
    {
        // Arrange
        const string value = "test-value";
        const string expectedValue = "'test-value'";
        IGithubExpression expression = new StringExpression(value);

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Test]
    public void LiteralExpressionExpression_Writes_ExpectedValue()
    {
        // Arrange
        const string value = "test-value";
        var expression = new LiteralExpression(value);

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe(value);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void BoolExpression_Writes_ExpectedValue(bool value)
    {
        // Arrange
        var expression = new BoolExpression(value);

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe(value
            .ToString()
            .ToLowerInvariant());
    }

    [Test]
    public void NullExpression_Writes_Null()
    {
        // Arrange
        var expression = new NullExpression();

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("null");
    }

    [TestCase(1)]
    [TestCase(123)]
    [TestCase(123.456)]
    [TestCase(0.123)]
    [TestCase(-123.456)]
    public void NumberExpression_Writes_ExpectedValue(double value)
    {
        // Arrange
        var expression = new NumberExpression(value);

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe(value.ToString(CultureInfo.InvariantCulture));
    }

    [TestCase("test")]
    [TestCase("test-value")]
    [TestCase("123")]
    [TestCase("test'value")]
    public void StringExpression_Writes_ExpectedValue(string value)
    {
        // Arrange
        var expression = new StringExpression(value);

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe($"'{value.Replace("'", "''")}'");
    }

    [Test]
    public void LogicalGroupingExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new LogicalGroupingExpression("value1");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("(value1)");
    }

    [Test]
    public void IndexExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new IndexedExpression("value1");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("[value1]");
    }

    [Test]
    public void PropertyDereferenceExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new DereferenceExpression("value1", "property1", "property2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("value1.property1.property2");
    }

    [Test]
    public void NotExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new NotExpression("value1");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("!value1");
    }

    [Test]
    public void LessThanExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new LessThanExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("value1 < value2");
    }

    [Test]
    public void LessThanOrEqualExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new LessThanOrEqualExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("value1 <= value2");
    }

    [Test]
    public void GreaterThanExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new GreaterThanExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("value1 > value2");
    }

    [Test]
    public void GreaterThanOrEqualExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new GreaterThanOrEqualExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("value1 >= value2");
    }

    [Test]
    public void EqualExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new EqualExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("value1 == value2");
    }

    [Test]
    public void NotEqualExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new NotEqualExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("value1 != value2");
    }

    [Test]
    public void AndExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new AndExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("value1 && value2");
    }

    [Test]
    public void OrExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new OrExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("value1 || value2");
    }

    [Test]
    public void ContainsExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new ContainsExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("contains(value1, value2)");
    }

    [Test]
    public void StartsWithExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new StartsWithExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("startsWith(value1, value2)");
    }

    [Test]
    public void EndsWithExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new EndsWithExpression("value1", "value2");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("endsWith(value1, value2)");
    }

    [Test]
    public void FormatExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new FormatExpression("'Hello {0} {1} {2}'", "Mona", "the", "Octocat");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("format('Hello {0} {1} {2}', Mona, the, Octocat)");
    }

    [Test]
    public void JoinExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new JoinExpression("[value1, value2]", "', '");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("join([value1, value2], ', ')");
    }

    [Test]
    public void ToJsonExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new ToJsonExpression("value1");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("toJSON(value1)");
    }

    [Test]
    public void FromJsonExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new FromJsonExpression("value1");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("fromJSON(value1)");
    }

    [Test]
    public void HashFilesExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new HashFilesExpression("**/package-lock.json");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("hashFiles(**/package-lock.json)");
    }

    [Test]
    public void SuccessExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new SuccessExpression();

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("success()");
    }

    [Test]
    public void AlwaysExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new AlwaysExpression();

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("always()");
    }

    [Test]
    public void CancelledExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new CancelledExpression();

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("cancelled()");
    }

    [Test]
    public void FailureExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new FailureExpression();

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("failure()");
    }

    [Test]
    public void ConsumedVariableExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new ConsumedVariableExpression("jobName", "variableName");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("needs.jobName.outputs.variableName");
    }

    [Test]
    public void ConsumedResultExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new ConsumedResultExpression("jobName");

        // Act
        var result = expression.ToString();

        // Assert
        result.ShouldBe("needs.jobName.result");
    }
}
