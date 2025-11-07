using CryptoFairPicker.Csprng;
using NUnit.Framework;

namespace CryptoFairPicker.Tests.Csprng;

public class CsprngWinnerSelectorTests
{
    [Test]
    public void PickWinner_ReturnsValueInCorrectRange()
    {
        // Arrange
        var selector = new CsprngWinnerSelector();
        var round = RoundId.FromRound(1000);

        // Act
        var winner = selector.PickWinner(10, round);

        // Assert
        Assert.That(winner, Is.InRange(1, 10));
    }

    [Test]
    public async Task PickWinnerAsync_ReturnsValueInCorrectRange()
    {
        // Arrange
        var selector = new CsprngWinnerSelector();
        var round = RoundId.FromRound(1000);

        // Act
        var winner = await selector.PickWinnerAsync(10, round);

        // Assert
        Assert.That(winner, Is.InRange(1, 10));
    }

    [Test]
    public void PickWinner_ThrowsForInvalidN()
    {
        // Arrange
        var selector = new CsprngWinnerSelector();
        var round = RoundId.FromRound(1000);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => selector.PickWinner(0, round));
        Assert.Throws<ArgumentException>(() => selector.PickWinner(-1, round));
    }

    [Test]
    public void PickWinner_ThrowsForNullRound()
    {
        // Arrange
        var selector = new CsprngWinnerSelector();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => selector.PickWinner(10, null!));
    }

    [Test]
    public void PickWinner_ProducesVariedResults()
    {
        // Arrange
        var selector = new CsprngWinnerSelector();
        var round = RoundId.FromRound(1000);
        var results = new HashSet<int>();

        // Act - Generate many values
        for (int i = 0; i < 100; i++)
        {
            results.Add(selector.PickWinner(100, round));
        }

        // Assert - Should get varied results
        Assert.That(results.Count, Is.GreaterThan(50), "CSPRNG selector should produce varied results");
    }
}
