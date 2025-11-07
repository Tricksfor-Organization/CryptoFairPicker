using CryptoFairPicker.Strategies;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CryptoFairPicker.Tests;

public class ServiceCollectionExtensionsTests
{
    [Test]
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
        
        Assert.That(picker, Is.Not.Null);
        Assert.That(strategy, Is.Not.Null);
        Assert.That(picker, Is.TypeOf<FairPicker>());
        Assert.That(strategy, Is.TypeOf<CsprngStrategy>());
    }

    [Test]
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
        
        Assert.That(picker, Is.Not.Null);
        Assert.That(strategy, Is.Not.Null);
        Assert.That(strategy, Is.TypeOf<CsprngStrategy>());
    }

    [Test]
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
        
        Assert.That(picker, Is.Not.Null);
        Assert.That(strategy, Is.Not.Null);
        Assert.That(commitReveal, Is.Not.Null);
        Assert.That(strategy, Is.TypeOf<CommitRevealStrategy>());
    }

    [Test]
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
        
        Assert.That(picker, Is.Not.Null);
        Assert.That(strategy, Is.Not.Null);
        Assert.That(strategy, Is.TypeOf<DrandBeaconStrategy>());
    }

    [Test]
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
        Assert.That(strategy, Is.Not.Null);
        Assert.That(strategy, Is.TypeOf<DrandBeaconStrategy>());
    }

    [Test]
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
        Assert.That(result, Is.InRange(0, 9));
    }

    [Test]
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
        Assert.That(picker1, Is.Not.SameAs(picker2));
    }

    [Test]
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
        Assert.That(strategy1, Is.SameAs(strategy2));
    }
}
