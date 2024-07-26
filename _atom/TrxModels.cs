namespace Atom;

[XmlRoot(ElementName = "TestRun", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
public sealed record TestRun
{
    [XmlAttribute(AttributeName = "id")]
    public string Id { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "runUser")]
    public string RunUser { get; init; } = string.Empty;

    [XmlElement(ElementName = "Times")]
    public Times Times { get; init; } = new();

    [XmlElement(ElementName = "TestSettings")]
    public TestSettings TestSettings { get; init; } = new();

    [XmlArray(ElementName = "Results")]
    [XmlArrayItem(ElementName = "UnitTestResult")]
    public List<UnitTestResult> Results { get; init; } = [];

    [XmlArray(ElementName = "TestDefinitions")]
    [XmlArrayItem(ElementName = "UnitTest")]
    public List<UnitTest> TestDefinitions { get; init; } = [];

    [XmlArray(ElementName = "TestEntries")]
    [XmlArrayItem(ElementName = "TestEntry")]
    public List<TestEntry> TestEntries { get; init; } = [];

    [XmlArray(ElementName = "TestLists")]
    [XmlArrayItem(ElementName = "TestList")]
    public List<TestList> TestLists { get; init; } = [];

    [XmlElement(ElementName = "ResultSummary")]
    public ResultSummary ResultSummary { get; init; } = new();
}

public sealed record Times
{
    [XmlAttribute(AttributeName = "creation")]
    public DateTime Creation { get; init; }

    [XmlAttribute(AttributeName = "queuing")]
    public DateTime Queuing { get; init; }

    [XmlAttribute(AttributeName = "start")]
    public DateTime Start { get; init; }

    [XmlAttribute(AttributeName = "finish")]
    public DateTime Finish { get; init; }
}

public sealed record TestSettings
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "id")]
    public string Id { get; init; } = string.Empty;

    [XmlElement(ElementName = "Deployment")]
    public Deployment Deployment { get; init; } = new();
}

public sealed record Deployment
{
    [XmlAttribute(AttributeName = "runDeploymentRoot")]
    public string RunDeploymentRoot { get; init; } = string.Empty;
}

public sealed record UnitTestResult
{
    [XmlAttribute(AttributeName = "executionId")]
    public string ExecutionId { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "testId")]
    public string TestId { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "testName")]
    public string TestName { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "computerName")]
    public string ComputerName { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "duration")]
    public string Duration { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "startTime")]
    public DateTime StartTime { get; init; }

    [XmlAttribute(AttributeName = "endTime")]
    public DateTime EndTime { get; init; }

    [XmlAttribute(AttributeName = "testType")]
    public string TestType { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "outcome")]
    public string Outcome { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "testListId")]
    public string TestListId { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "relativeResultsDirectory")]
    public string RelativeResultsDirectory { get; init; } = string.Empty;

    [XmlElement(ElementName = "Output")]
    public ResultOutput Output { get; init; } = new();
}

public sealed record ResultOutput
{
    [XmlElement(ElementName = "ErrorInfo")]
    public ErrorInfo ErrorInfo { get; init; } = new();
}

public sealed record ErrorInfo
{
    [XmlElement(ElementName = "Message")]
    public string Message { get; init; } = string.Empty;

    [XmlElement(ElementName = "StackTrace")]
    public string StackTrace { get; init; } = string.Empty;

    [XmlElement(ElementName = "StdOut")]
    public string StdOut { get; init; } = string.Empty;

    [XmlElement(ElementName = "StdErr")]
    public string StdErr { get; init; } = string.Empty;
}

public sealed record UnitTest
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "storage")]
    public string Storage { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "id")]
    public string Id { get; init; } = string.Empty;

    [XmlElement(ElementName = "Execution")]
    public Execution Execution { get; init; } = new();

    [XmlElement(ElementName = "TestMethod")]
    public TestMethod TestMethod { get; init; } = new();
}

public sealed record Execution
{
    [XmlAttribute(AttributeName = "id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record TestMethod
{
    [XmlAttribute(AttributeName = "codeBase")]
    public string CodeBase { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "adapterTypeName")]
    public string AdapterTypeName { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "className")]
    public string ClassName { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; init; } = string.Empty;
}

public sealed record TestEntry
{
    [XmlAttribute(AttributeName = "testId")]
    public string TestId { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "executionId")]
    public string ExecutionId { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "testListId")]
    public string TestListId { get; init; } = string.Empty;
}

public sealed record TestList
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "id")]
    public string Id { get; init; } = string.Empty;
}

public sealed record ResultSummary
{
    [XmlAttribute(AttributeName = "outcome")]
    public string Outcome { get; init; } = string.Empty;

    [XmlElement(ElementName = "Counters")]
    public Counters Counters { get; init; } = new();

    [XmlElement(ElementName = "Output")]
    public Output Output { get; init; } = new();

    [XmlArray(ElementName = "CollectorDataEntries")]
    [XmlArrayItem(ElementName = "Collector")]
    public List<Collector> CollectorDataEntries { get; init; } = [];
}

public sealed record Counters
{
    [XmlAttribute(AttributeName = "total")]
    public int Total { get; init; }

    [XmlAttribute(AttributeName = "executed")]
    public int Executed { get; init; }

    [XmlAttribute(AttributeName = "passed")]
    public int Passed { get; init; }

    [XmlAttribute(AttributeName = "failed")]
    public int Failed { get; init; }

    [XmlAttribute(AttributeName = "error")]
    public int Error { get; init; }

    [XmlAttribute(AttributeName = "timeout")]
    public int Timeout { get; init; }

    [XmlAttribute(AttributeName = "aborted")]
    public int Aborted { get; init; }

    [XmlAttribute(AttributeName = "inconclusive")]
    public int Inconclusive { get; init; }

    [XmlAttribute(AttributeName = "passedButRunAborted")]
    public int PassedButRunAborted { get; init; }

    [XmlAttribute(AttributeName = "notRunnable")]
    public int NotRunnable { get; init; }

    [XmlAttribute(AttributeName = "notExecuted")]
    public int NotExecuted { get; init; }

    [XmlAttribute(AttributeName = "disconnected")]
    public int Disconnected { get; init; }

    [XmlAttribute(AttributeName = "warning")]
    public int Warning { get; init; }

    [XmlAttribute(AttributeName = "completed")]
    public int Completed { get; init; }

    [XmlAttribute(AttributeName = "inProgress")]
    public int InProgress { get; init; }

    [XmlAttribute(AttributeName = "pending")]
    public int Pending { get; init; }
}

public sealed record Output
{
    [XmlElement(ElementName = "StdOut")]
    public string StdOut { get; init; } = string.Empty;
}

public sealed record Collector
{
    [XmlAttribute(AttributeName = "agentName")]
    public string AgentName { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "uri")]
    public string Uri { get; init; } = string.Empty;

    [XmlAttribute(AttributeName = "collectorDisplayName")]
    public string CollectorDisplayName { get; init; } = string.Empty;

    [XmlArray(ElementName = "UriAttachments")]
    [XmlArrayItem(ElementName = "UriAttachment")]
    public List<UriAttachment> UriAttachments { get; init; } = [];
}

public sealed record UriAttachment
{
    [XmlElement(ElementName = "A")]
    public A A { get; init; } = new();
}

public sealed record A
{
    [XmlAttribute(AttributeName = "href")]
    public string Href { get; init; } = string.Empty;
}