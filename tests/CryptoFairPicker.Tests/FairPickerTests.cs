using CryptoFairPicker.Strategies;
using NUnit.Framework;

namespace CryptoFairPicker.Tests;

public class FairPickerTests
{
    [Test]
    public void Constructor_ThrowsForNullStrategy()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FairPicker(null!));
    }

    [Test]
    public void PickWinner_DelegatesToStrategy()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        var picker = new FairPicker(strategy);
        const int optionCount = 10;

        // Act
        var result = picker.PickWinner(optionCount);

        // Assert
        Assert.That(result, Is.InRange(0, optionCount - 1));
    }

    [Test]
    public async Task PickWinnerAsync_DelegatesToStrategy()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        var picker = new FairPicker(strategy);
        const int optionCount = 10;

        // Act
        var result = await picker.PickWinnerAsync(optionCount);

        // Assert
        Assert.That(result, Is.InRange(0, optionCount - 1));
    }

    [Test]
    public void PickWinner_ThrowsForInvalidOptionCount()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        var picker = new FairPicker(strategy);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => picker.PickWinner(0));
        Assert.Throws<ArgumentException>(() => picker.PickWinner(-1));
    }

    [Test]
    public void PickWinnerAsync_ThrowsForInvalidOptionCount()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        var picker = new FairPicker(strategy);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await picker.PickWinnerAsync(0));
        Assert.ThrowsAsync<ArgumentException>(async () => await picker.PickWinnerAsync(-1));
    }

    [Test]
    public void PickWinner_WorksWithCommitRevealStrategy()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();
        var picker = new FairPicker(strategy);
        const int optionCount = 10;

        // Act
        var result = picker.PickWinner(optionCount);

        // Assert
        Assert.That(result, Is.InRange(0, optionCount - 1));
        Assert.That(strategy.CommitmentHash, Is.Not.Null);
    }

    [Test]
    public void PickWinner_ProducesUniformDistribution()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        var picker = new FairPicker(strategy);
        const int optionCount = 5;
        const int iterations = 5000;
        var counts = new int[optionCount];

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var result = picker.PickWinner(optionCount);
            counts[result]++;
        }

        // Assert - Each option should appear roughly equal times (within 25% of expected)
        var expected = iterations / optionCount;
        var tolerance = expected * 0.25;
        
        foreach (var count in counts)
        {
            Assert.That(count, Is.InRange(expected - tolerance, expected + tolerance));
        }
    }
}
