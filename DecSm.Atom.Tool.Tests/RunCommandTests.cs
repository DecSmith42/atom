namespace DecSm.Atom.Tool.Tests;

[TestFixture]
[SuppressMessage("System.IO.Abstractions", "IO0006:Replace Path class with IFileSystem.Path for improved testability")]
public class RunCommandTests
{
    private MockFileSystem _fileSystem = null!;
    private string _currentDir = null!;
    private string _parentDir = null!;
    private string _root = null!;

    [SetUp]
    public void SetUp()
    {
        _root = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\"
            : "/";

        _parentDir = Path.Combine(_root, "parent");
        _currentDir = Path.Combine(_parentDir, "tests");

        RunCommand.MockDotnetCli = true;
    }

    [TearDown]
    public void TearDown()
    {
        RunCommand.MockDotnetCli = false;
        RunCommand.FileSystem = new FileSystem();
    }

    private void SetupFileSystem(Dictionary<string, MockFileData> files)
    {
        _fileSystem = new(files, _currentDir);

        // Ensure current directory exists if not implicitly created by files
        if (!_fileSystem.Directory.Exists(_currentDir))
            _fileSystem.AddDirectory(_currentDir);

        RunCommand.FileSystem = _fileSystem;
    }

    [Test]
    public async Task Handle_WithEmptySubject_FindsDefaultProject_InCurrentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_currentDir, "_atom.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithEmptySubject_FindsDefaultProject_InParentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_parentDir, "_build.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithEmptySubject_FindsDefaultProject_InRootDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_root, "_atom.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithEmptySubject_FindsDefaultNestedProject_InParentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_parentDir, "_atom", "_atom.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithEmptySubject_FindsDefaultCsFile_InCurrentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_currentDir, "Atom.cs"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithEmptySubject_FindsDefaultCsFile_InParentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_parentDir, "Atom.cs"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithKnownProject_FindsProject_InCurrentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_currentDir, "mybuild.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "mybuild.csproj", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithKnownProject_FindsProject_InParentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_parentDir, "mybuild.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "mybuild.csproj", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithKnownProject_FindsNestedProject_InParentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_parentDir, "mybuild", "mybuild.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "mybuild.csproj", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithKnownProjectName_FindsProject_InCurrentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_currentDir, "mybuild.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "mybuild", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithKnownProjectName_FindsProject_InParentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_parentDir, "mybuild.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "mybuild", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithKnownProjectName_FindsNestedProject_InParentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_parentDir, "mybuild", "mybuild.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "mybuild", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithKnownProjectName_FindsCsFile_InParentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_parentDir, "mybuild.cs"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "mybuild", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithKnownCsFile_FindsCsFile_InCurrentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_currentDir, "mybuild.cs"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "mybuild.cs", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithKnownCsFile_FindsCsFile_InParentDirectory()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_parentDir, "mybuild.cs"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "mybuild.cs", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_WithNestedProject_FindsProject()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_currentDir, "mybuild", "mybuild.csproj"), new("") },
        };

        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "mybuild", CancellationToken.None);

        result.ShouldBe(0);
    }

    [Test]
    public async Task Handle_FileNotFound_ReturnsError()
    {
        var files = new Dictionary<string, MockFileData>();
        SetupFileSystem(files);

        var result = await RunCommand.Handle([], "nonexistent", CancellationToken.None);

        result.ShouldBe(1);
    }

    [Test]
    public async Task Handle_WithArgs_RunsSuccessfully()
    {
        var files = new Dictionary<string, MockFileData>
        {
            { Path.Combine(_currentDir, "_atom.csproj"), new("") },
        };

        SetupFileSystem(files);

        var args = new[] { "arg1", "arg with space", "arg\nwith\nnewline" };

        var result = await RunCommand.Handle(args, "", CancellationToken.None);

        result.ShouldBe(0);
    }
}
