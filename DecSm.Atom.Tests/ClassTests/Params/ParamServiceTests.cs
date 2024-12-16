namespace DecSm.Atom.Tests.ClassTests.Params;

[TestFixture]
[NonParallelizable]
public class ParamServiceTests
{
    [SetUp]
    public void Setup()
    {
        _buildDefinition = A.Fake<IBuildDefinition>();
        _args = new(true, Array.Empty<CommandArg>());
        _config = A.Fake<IConfiguration>();

        _vaultProviders = new List<ISecretsProvider>
        {
            A.Fake<ISecretsProvider>(),
        };

        _paramService = new(_buildDefinition, _args, _config, _vaultProviders);
    }

    [TearDown]
    public void TearDown() =>
        Environment.SetEnvironmentVariable("test-param", null);

    private IBuildDefinition _buildDefinition;
    private CommandLineArgs _args;
    private IConfiguration _config;
    private IEnumerable<ISecretsProvider> _vaultProviders;
    private ParamService _paramService;

    private static string TestParam => "TestParam";

    [Test]
    public void GetParam_WithExpression_ReturnsExpectedValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = false,
        };

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
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = false,
        };

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
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = false,
        };

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
    }

    [Test]
    public void GetParam_WithVaultValue_ReturnsExpectedValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = true,
        };

        var vaultProvider = A.Fake<ISecretsProvider>();
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
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = false,
        };

        var vaultProvider = A.Fake<ISecretsProvider>();
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
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = true,
        };

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
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = false,
        };
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

    [Test]
    public void GetParam_WithNoCacheScope_DoesNotCacheValue()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.All,
            IsSecret = false,
        };

        _config = new ConfigurationBuilder().Build();

        _paramService = new(_buildDefinition, _args, _config, _vaultProviders);

        A
            .CallTo(() => _buildDefinition.ParamDefinitions)
            .Returns(new Dictionary<string, ParamDefinition>
            {
                { "TestParam", paramDefinition },
            });

        // Act
        string? result1;
        string? result2;

        using (_paramService.CreateNoCacheScope())
        {
            result1 = _paramService.GetParam("TestParam", "DefaultValue1");
            result2 = _paramService.GetParam("TestParam", "DefaultValue2");
        }

        // Act
        var result3 = _paramService.GetParam("TestParam", "DefaultValue3");
        var result4 = _paramService.GetParam("TestParam", "DefaultValue4");

        // Assert
        result1.ShouldBe("DefaultValue1");
        result2.ShouldBe("DefaultValue2");
        result3.ShouldBe("DefaultValue3");
        result4.ShouldBe("DefaultValue3");
    }

    [Test]
    public void GetParam_WithNoneFilter_IncludesNone()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.None,
            IsSecret = false,
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

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
        result.ShouldBe("DefaultValue");
    }

    [Test]
    public void GetParam_WithCommandLineArgsFilter_IncludesCommandLineArgs()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.CommandLineArgs,
            IsSecret = false,
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

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
        result.ShouldBe("ArgValue");
    }

    [Test]
    public void GetParam_WithEnvironmentVariablesFilter_IncludesEnvironmentVariables()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.EnvironmentVariables,
            IsSecret = false,
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

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
        result.ShouldBe("EnvValue");
    }

    [Test]
    public void GetParam_WithConfigurationFilter_IncludesConfiguration()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.Configuration,
            IsSecret = false,
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

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
    public void GetParam_WithVaultFilter_IncludesVault()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.Secrets,
            IsSecret = true,
        };

        _args = new(true, [new ParamArg("test-param", "NotTestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

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
        result.ShouldBe("VaultValue");
    }

    [Test]
    public void GetParam_WithSecretsFilter_IncludesVault()
    {
        // Arrange
        var paramDefinition = new ParamDefinition("TestParam")
        {
            ArgName = "test-param",
            Description = "Test parameter",
            DefaultValue = null,
            Sources = ParamSource.Secrets,
            IsSecret = true,
        };

        _args = new(true, [new ParamArg("test-param", "TestParam", "ArgValue")]);
        Environment.SetEnvironmentVariable("test-param", "EnvValue");

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Params:test-param", "ConfigValue" },
            })
            .Build();

        _vaultProviders = [new TestSecretsProvider()];

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
        result.ShouldBe("VaultValue");
    }

    private class TestSecretsProvider : ISecretsProvider
    {
        public string GetSecret(string secretName) =>
            "VaultValue";
    }
}
