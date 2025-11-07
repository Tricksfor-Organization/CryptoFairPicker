using CryptoFairPicker.Strategies;
using NUnit.Framework;

namespace CryptoFairPicker.Tests;

public class CsprngStrategyTests
{
    [Test]
    public void Pick_ReturnsValueInRange()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        const int optionCount = 10;

        // Act
        var result = strategy.Pick(optionCount);

        // Assert
        Assert.That(result, Is.InRange(0, optionCount - 1));
    }

    [Test]
    public void Pick_ThrowsForInvalidOptionCount()
    {
        // Arrange
        var strategy = new CsprngStrategy();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => strategy.Pick(0));
        Assert.Throws<ArgumentException>(() => strategy.Pick(-1));
    }

    [Test]
    public async Task PickAsync_ReturnsValueInRange()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        const int optionCount = 10;

        // Act
        var result = await strategy.PickAsync(optionCount);

        // Assert
        Assert.That(result, Is.InRange(0, optionCount - 1));
    }

    [Test]
    public void Pick_ProducesUniformDistribution()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        const int optionCount = 10;
        const int iterations = 10000;
        var counts = new int[optionCount];

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var result = strategy.Pick(optionCount);
            counts[result]++;
        }

        // Assert - Each option should appear roughly equal times (within 20% of expected)
        var expected = iterations / optionCount;
        var tolerance = expected * 0.20; // 20% tolerance
        
        foreach (var count in counts)
        {
            Assert.That(count, Is.InRange(expected - tolerance, expected + tolerance));
        }
    }

    [Test]
    public void Pick_ProducesDifferentValuesOverMultipleCalls()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        const int optionCount = 100;
        var results = new HashSet<int>();

        // Act
        for (int i = 0; i < 50; i++)
        {
            results.Add(strategy.Pick(optionCount));
        }

        // Assert - Should have produced at least some different values
        Assert.That(results.Count, Is.GreaterThan(10), "Should produce varied results");
    }
}
