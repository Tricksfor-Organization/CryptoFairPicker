using CryptoFairPicker.Drand;
using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Models;
using NSubstitute;
using NUnit.Framework;

namespace CryptoFairPicker.Tests.Drand;

public class DrandWinnerSelectorTests
{
    [Test]
    public async Task PickWinnerAsync_ReturnsValueInCorrectRange()
    {
        // Arrange
        var mockSource = Substitute.For<IFairRandomSource>();
        mockSource
            .NextIntAsync(Arg.Any<int>(), Arg.Any<RoundId>(), Arg.Any<CancellationToken>())
            .Returns(5);
        
        var selector = new DrandWinnerSelector(mockSource);
        var round = RoundId.FromRound(1000);

        // Act
        var winner = await selector.PickWinnerAsync(10, round);

        // Assert
        Assert.That(winner, Is.EqualTo(6)); // 5 + 1 = 6 (1-indexed)
        await mockSource.Received(1).NextIntAsync(10, round, Arg.Any<CancellationToken>());
    }

    [Test]
    public void PickWinner_ReturnsValueInCorrectRange()
    {
        // Arrange
        var mockSource = Substitute.For<IFairRandomSource>();
        mockSource
            .NextInt(Arg.Any<int>(), Arg.Any<RoundId>())
            .Returns(3);
        
        var selector = new DrandWinnerSelector(mockSource);
        var round = RoundId.FromRound(1000);

        // Act
        var winner = selector.PickWinner(10, round);

        // Assert
        Assert.That(winner, Is.EqualTo(4)); // 3 + 1 = 4 (1-indexed)
        mockSource.Received(1).NextInt(10, round);
    }

    [Test]
    public async Task PickWinnerAsync_Returns1ForFirstIndex()
    {
        // Arrange
        var mockSource = Substitute.For<IFairRandomSource>();
        mockSource
            .NextIntAsync(Arg.Any<int>(), Arg.Any<RoundId>(), Arg.Any<CancellationToken>())
            .Returns(0);
        
        var selector = new DrandWinnerSelector(mockSource);
        var round = RoundId.FromRound(1000);

        // Act
        var winner = await selector.PickWinnerAsync(100, round);

        // Assert
        Assert.That(winner, Is.EqualTo(1)); // 0 + 1 = 1
    }

    [Test]
    public async Task PickWinnerAsync_ReturnsNForLastIndex()
    {
        // Arrange
        var mockSource = Substitute.For<IFairRandomSource>();
        mockSource
            .NextIntAsync(Arg.Any<int>(), Arg.Any<RoundId>(), Arg.Any<CancellationToken>())
            .Returns(99);
        
        var selector = new DrandWinnerSelector(mockSource);
        var round = RoundId.FromRound(1000);

        // Act
        var winner = await selector.PickWinnerAsync(100, round);

        // Assert
        Assert.That(winner, Is.EqualTo(100)); // 99 + 1 = 100
    }

    [Test]
    public void PickWinnerAsync_ThrowsForInvalidN()
    {
        // Arrange
        var mockSource = Substitute.For<IFairRandomSource>();
        var selector = new DrandWinnerSelector(mockSource);
        var round = RoundId.FromRound(1000);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await selector.PickWinnerAsync(0, round));
        Assert.ThrowsAsync<ArgumentException>(async () => await selector.PickWinnerAsync(-1, round));
    }

    [Test]
    public void PickWinnerAsync_ThrowsForNullRound()
    {
        // Arrange
        var mockSource = Substitute.For<IFairRandomSource>();
        var selector = new DrandWinnerSelector(mockSource);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await selector.PickWinnerAsync(10, null!));
    }

    [Test]
    public void Constructor_ThrowsForNullSource()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DrandWinnerSelector(null!));
    }
}
