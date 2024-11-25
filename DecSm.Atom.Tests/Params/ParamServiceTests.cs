using DecSm.Atom.Vaults;
using Microsoft.Extensions.Configuration;

namespace DecSm.Atom.Tests.Params;

[TestFixture]
public class ParamServiceTests
{
    [SetUp]
    public void Setup()
    {
        _buildDefinition = A.Fake<IBuildDefinition>();
        _args = new(true, Array.Empty<CommandArg>());
        _config = A.Fake<IConfiguration>();

        _vaultProviders = new List<IVaultProvider>
        {
            A.Fake<IVaultProvider>(),
        };

        _paramService = new(_buildDefinition, _args, _config, _vaultProviders);
    }

    private IBuildDefinition _buildDefinition;
    private CommandLineArgs _args;
    private IConfiguration _config;
    private IEnumerable<IVaultProvider> _vaultProviders;
    private ParamService _paramService;

    private string TestParam => "TestParam";

    [Test]
    public void GetParam_WithExpression_ReturnsExpectedValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam", new("test-param", "Test parameter"));

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _paramService = new(_buildDefinition, _args, _config, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        var result = _paramService.GetParam(() => TestParam, "DefaultValue");

        // Assert
        result.ShouldBe("ConfigValue");
    }

    [Test]
    public void GetParam_WithString_ReturnsExpectedValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam", new("test-param", "Test parameter"));

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _paramService = new(_buildDefinition, _args, _config, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("ConfigValue");
    }

    [Test]
    public void GetParam_WithEnvironmentVariable_ReturnsExpectedValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam", new("test-param", "Test parameter"));

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _paramService = new(_buildDefinition, _args, _config, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("EnvValue");

        // Cleanup
        Environment.SetEnvironmentVariable("test-param", null);
    }

    [Test]
    public void GetParam_WithVaultValue_ReturnsExpectedValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam", new SecretDefinitionAttribute("test-param", "Test parameter"));

        var vaultProvider = A.Fake<IVaultProvider>();
        _config = new ConfigurationBuilder().Build();
        _paramService = new(_buildDefinition, _args, _config, [vaultProvider]);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        A
            .CallTo(() => vaultProvider.GetSecret("test-param"))
            .Returns("VaultValue");

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("VaultValue");
    }

    [Test]
    public void GetParam_WithVaultValueButNotSecret_ReturnsDefaultValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam", new("test-param", "Test parameter"));

        var vaultProvider = A.Fake<IVaultProvider>();
        _config = new ConfigurationBuilder().Build();
        _paramService = new(_buildDefinition, _args, _config, [vaultProvider]);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        A
            .CallTo(() => vaultProvider.GetSecret("test-param"))
            .Returns("VaultValue");

        // Act
        var result = _paramService.GetParam("TestParam", "DefaultValue");

        // Assert
        result.ShouldBe("DefaultValue");
    }

    [Test]
    public void MaskSecrets_WithSecretsInText_MasksSecrets()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam", new SecretDefinitionAttribute("test-param", "Test parameter"));
        _config = new ConfigurationBuilder().Build();
        _paramService = new(_buildDefinition, _args, _config, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        A
            .CallTo(() => _vaultProviders
                .First()
                .GetSecret("test-param"))
            .Returns("SecretValue");

        _paramService.GetParam("TestParam", "DefaultValue");

        // Act
        var result = _paramService.MaskSecrets("This is a SecretValue in the text.");

        // Assert
        result.ShouldBe("This is a ***** in the text.");
    }

    [Test]
    public void MaskSecrets_WithSecretsInTextButNotSecretAttribute_DoesNotMaskSecrets()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam", new("test-param", "Test parameter"));
        _config = new ConfigurationBuilder().Build();
        _paramService = new(_buildDefinition, _args, _config, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        A
            .CallTo(() => _vaultProviders
                .First()
                .GetSecret("test-param"))
            .Returns("NotSecretValue");

        _paramService.GetParam("TestParam", "DefaultValue");

        // Act
        var result = _paramService.MaskSecrets("This is a NotSecretValue in the text.");

        // Assert
        result.ShouldBe("This is a NotSecretValue in the text.");
    }
}
