namespace Atom;

[XmlRoot(ElementName = "TestRun", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
public sealed record TestRun
{
    [XmlAttribute(AttributeName = "id")]
    public string Id { get; init; }

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; init; }

    [XmlAttribute(AttributeName = "runUser")]
    public string RunUser { get; init; }

    [XmlElement(ElementName = "Times")]
    public Times Times { get; init; }

    [XmlElement(ElementName = "TestSettings")]
    public TestSettings TestSettings { get; init; }

    [XmlArray(ElementName = "Results")]
    [XmlArrayItem(ElementName = "UnitTestResult")]
    public List<UnitTestResult> Results { get; init; }

    [XmlArray(ElementName = "TestDefinitions")]
    [XmlArrayItem(ElementName = "UnitTest")]
    public List<UnitTest> TestDefinitions { get; init; }

    [XmlArray(ElementName = "TestEntries")]
    [XmlArrayItem(ElementName = "TestEntry")]
    public List<TestEntry> TestEntries { get; init; }

    [XmlArray(ElementName = "TestLists")]
    [XmlArrayItem(ElementName = "TestList")]
    public List<TestList> TestLists { get; init; }

    [XmlElement(ElementName = "ResultSummary")]
    public ResultSummary ResultSummary { get; init; }
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
    public string Name { get; init; }

    [XmlAttribute(AttributeName = "id")]
    public string Id { get; init; }

    [XmlElement(ElementName = "Deployment")]
    public Deployment Deployment { get; init; }
}

public sealed record Deployment
{
    [XmlAttribute(AttributeName = "runDeploymentRoot")]
    public string RunDeploymentRoot { get; init; }
}

// Need to add an output model to the Result:
// <UnitTestResult executionId="9850d38b-048c-4680-90d1-5fbb51cf6e70" testId="dbd4de17-ae73-87b4-5efb-67309565b603" testName="ShouldFail" computerName="DEC-PC" duration="00:00:00.0150750" startTime="2024-07-15T22:25:38.7151239+10:00" endTime="2024-07-15T22:25:38.7301990+10:00" testType="13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" outcome="Failed" testListId="8c84fa94-04c1-424b-9868-57a2d4851a1d" relativeResultsDirectory="9850d38b-048c-4680-90d1-5fbb51cf6e70">
//     <Output>
//     <ErrorInfo>
//     <Message>This test should fail.</Message>
//     <StackTrace>   at DecSm.Atom.Tests.TestFailure.ShouldFail() in L:\DecSm\Atom\DecSm.Atom.Tests\TestFailure.cs:line 9&#xD;
//
// 1)    at DecSm.Atom.Tests.TestFailure.ShouldFail() in L:\DecSm\Atom\DecSm.Atom.Tests\TestFailure.cs:line 9&#xD;
//
//     </StackTrace>
//     </ErrorInfo>
//     </Output>
//     </UnitTestResult>

public sealed record UnitTestResult
{
    [XmlAttribute(AttributeName = "executionId")]
    public string ExecutionId { get; init; }

    [XmlAttribute(AttributeName = "testId")]
    public string TestId { get; init; }

    [XmlAttribute(AttributeName = "testName")]
    public string TestName { get; init; }

    [XmlAttribute(AttributeName = "computerName")]
    public string ComputerName { get; init; }

    [XmlAttribute(AttributeName = "duration")]
    public string Duration { get; init; }

    [XmlAttribute(AttributeName = "startTime")]
    public DateTime StartTime { get; init; }

    [XmlAttribute(AttributeName = "endTime")]
    public DateTime EndTime { get; init; }

    [XmlAttribute(AttributeName = "testType")]
    public string TestType { get; init; }

    [XmlAttribute(AttributeName = "outcome")]
    public string Outcome { get; init; }

    [XmlAttribute(AttributeName = "testListId")]
    public string TestListId { get; init; }

    [XmlAttribute(AttributeName = "relativeResultsDirectory")]
    public string RelativeResultsDirectory { get; init; }

    [XmlElement(ElementName = "Output")]
    public ResultOutput Output { get; init; }
}

public sealed record ResultOutput
{
    [XmlElement(ElementName = "ErrorInfo")]
    public ErrorInfo ErrorInfo { get; init; }
}

public sealed record ErrorInfo
{
    [XmlElement(ElementName = "Message")]
    public string Message { get; init; }

    [XmlElement(ElementName = "StackTrace")]
    public string StackTrace { get; init; }

    [XmlElement(ElementName = "StdOut")]
    public string StdOut { get; init; }

    [XmlElement(ElementName = "StdErr")]
    public string StdErr { get; init; }
}

public sealed record UnitTest
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; init; }

    [XmlAttribute(AttributeName = "storage")]
    public string Storage { get; init; }

    [XmlAttribute(AttributeName = "id")]
    public string Id { get; init; }

    [XmlElement(ElementName = "Execution")]
    public Execution Execution { get; init; }

    [XmlElement(ElementName = "TestMethod")]
    public TestMethod TestMethod { get; init; }
}

public sealed record Execution
{
    [XmlAttribute(AttributeName = "id")]
    public string Id { get; init; }
}

public sealed record TestMethod
{
    [XmlAttribute(AttributeName = "codeBase")]
    public string CodeBase { get; init; }

    [XmlAttribute(AttributeName = "adapterTypeName")]
    public string AdapterTypeName { get; init; }

    [XmlAttribute(AttributeName = "className")]
    public string ClassName { get; init; }

    [XmlAttribute(AttributeName = "name")]
    public string Name { get; init; }
}

public sealed record TestEntry
{
    [XmlAttribute(AttributeName = "testId")]
    public string TestId { get; init; }

    [XmlAttribute(AttributeName = "executionId")]
    public string ExecutionId { get; init; }

    [XmlAttribute(AttributeName = "testListId")]
    public string TestListId { get; init; }
}

public sealed record TestList
{
    [XmlAttribute(AttributeName = "name")]
    public string Name { get; init; }

    [XmlAttribute(AttributeName = "id")]
    public string Id { get; init; }
}

public sealed record ResultSummary
{
    [XmlAttribute(AttributeName = "outcome")]
    public string Outcome { get; init; }

    [XmlElement(ElementName = "Counters")]
    public Counters Counters { get; init; }

    [XmlElement(ElementName = "Output")]
    public Output Output { get; init; }

    [XmlArray(ElementName = "CollectorDataEntries")]
    [XmlArrayItem(ElementName = "Collector")]
    public List<Collector> CollectorDataEntries { get; init; }
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
    public string StdOut { get; init; }
}

public sealed record Collector
{
    [XmlAttribute(AttributeName = "agentName")]
    public string AgentName { get; init; }

    [XmlAttribute(AttributeName = "uri")]
    public string Uri { get; init; }

    [XmlAttribute(AttributeName = "collectorDisplayName")]
    public string CollectorDisplayName { get; init; }

    [XmlArray(ElementName = "UriAttachments")]
    [XmlArrayItem(ElementName = "UriAttachment")]
    public List<UriAttachment> UriAttachments { get; init; }
}

public sealed record UriAttachment
{
    [XmlElement(ElementName = "A")]
    public A A { get; init; }
}

public sealed record A
{
    [XmlAttribute(AttributeName = "href")]
    public string Href { get; init; }
}