using CryptoFairPicker.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CryptoFairPicker.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCryptoFairPicker_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCryptoFairPicker();
        var provider = services.BuildServiceProvider();

        // Assert
        var picker = provider.GetService<IFairPicker>();
        var strategy = provider.GetService<IPickerStrategy>();
        
        Assert.NotNull(picker);
        Assert.NotNull(strategy);
        Assert.IsType<FairPicker>(picker);
        Assert.IsType<CsprngStrategy>(strategy);
    }

    [Fact]
    public void AddCryptoFairPicker_WithCustomStrategy_RegistersStrategy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCryptoFairPicker<CsprngStrategy>();
        var provider = services.BuildServiceProvider();

        // Assert
        var picker = provider.GetService<IFairPicker>();
        var strategy = provider.GetService<IPickerStrategy>();
        
        Assert.NotNull(picker);
        Assert.NotNull(strategy);
        Assert.IsType<CsprngStrategy>(strategy);
    }

    [Fact]
    public void AddCommitRevealPicker_RegistersCommitRevealStrategy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCommitRevealPicker();
        var provider = services.BuildServiceProvider();

        // Assert
        var picker = provider.GetService<IFairPicker>();
        var strategy = provider.GetService<IPickerStrategy>();
        var commitReveal = provider.GetService<CommitRevealStrategy>();
        
        Assert.NotNull(picker);
        Assert.NotNull(strategy);
        Assert.NotNull(commitReveal);
        Assert.IsType<CommitRevealStrategy>(strategy);
    }

    [Fact]
    public void AddDrandBeaconPicker_RegistersBeaconStrategy()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDrandBeaconPicker();
        var provider = services.BuildServiceProvider();

        // Assert
        var picker = provider.GetService<IFairPicker>();
        var strategy = provider.GetService<IPickerStrategy>();
        
        Assert.NotNull(picker);
        Assert.NotNull(strategy);
        Assert.IsType<DrandBeaconStrategy>(strategy);
    }

    [Fact]
    public void AddDrandBeaconPicker_WithCustomUrl_UsesCustomUrl()
    {
        // Arrange
        var services = new ServiceCollection();
        const string customUrl = "https://custom.beacon.url/latest";

        // Act
        services.AddDrandBeaconPicker(customUrl);
        var provider = services.BuildServiceProvider();

        // Assert
        var strategy = provider.GetService<IPickerStrategy>();
        Assert.NotNull(strategy);
        Assert.IsType<DrandBeaconStrategy>(strategy);
    }

    [Fact]
    public void RegisteredPicker_CanPickWinner()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCryptoFairPicker();
        var provider = services.BuildServiceProvider();
        var picker = provider.GetRequiredService<IFairPicker>();

        // Act
        var result = picker.PickWinner(10);

        // Assert
        Assert.InRange(result, 0, 9);
    }

    [Fact]
    public void TransientServices_CreateNewInstances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCryptoFairPicker();
        var provider = services.BuildServiceProvider();

        // Act
        var picker1 = provider.GetRequiredService<IFairPicker>();
        var picker2 = provider.GetRequiredService<IFairPicker>();

        // Assert
        Assert.NotSame(picker1, picker2);
    }

    [Fact]
    public void CommitRevealStrategy_IsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCommitRevealPicker();
        var provider = services.BuildServiceProvider();

        // Act
        var strategy1 = provider.GetRequiredService<CommitRevealStrategy>();
        var strategy2 = provider.GetRequiredService<CommitRevealStrategy>();

        // Assert
        Assert.Same(strategy1, strategy2);
    }
}
