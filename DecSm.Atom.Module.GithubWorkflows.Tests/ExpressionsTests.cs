namespace DecSm.Atom.Module.GithubWorkflows.Tests;

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
    public void PropertyExpression_Writes_ExpectedValue()
    {
        // Arrange
        var expression = new PropertyExpression("value1", "property1", "property2");

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
