using CryptoFairPicker.Strategies;
using Xunit;

namespace CryptoFairPicker.Tests;

public class FairPickerTests
{
    [Fact]
    public void Constructor_ThrowsForNullStrategy()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FairPicker(null!));
    }

    [Fact]
    public void PickWinner_DelegatesToStrategy()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        var picker = new FairPicker(strategy);
        const int optionCount = 10;

        // Act
        var result = picker.PickWinner(optionCount);

        // Assert
        Assert.InRange(result, 0, optionCount - 1);
    }

    [Fact]
    public async Task PickWinnerAsync_DelegatesToStrategy()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        var picker = new FairPicker(strategy);
        const int optionCount = 10;

        // Act
        var result = await picker.PickWinnerAsync(optionCount);

        // Assert
        Assert.InRange(result, 0, optionCount - 1);
    }

    [Fact]
    public void PickWinner_ThrowsForInvalidOptionCount()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        var picker = new FairPicker(strategy);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => picker.PickWinner(0));
        Assert.Throws<ArgumentException>(() => picker.PickWinner(-1));
    }

    [Fact]
    public async Task PickWinnerAsync_ThrowsForInvalidOptionCount()
    {
        // Arrange
        var strategy = new CsprngStrategy();
        var picker = new FairPicker(strategy);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => picker.PickWinnerAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => picker.PickWinnerAsync(-1));
    }

    [Fact]
    public void PickWinner_WorksWithCommitRevealStrategy()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();
        var picker = new FairPicker(strategy);
        const int optionCount = 10;

        // Act
        var result = picker.PickWinner(optionCount);

        // Assert
        Assert.InRange(result, 0, optionCount - 1);
        Assert.NotNull(strategy.CommitmentHash);
    }

    [Fact]
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
            Assert.InRange(count, expected - tolerance, expected + tolerance);
        }
    }
}
