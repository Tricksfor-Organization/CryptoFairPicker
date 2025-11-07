using CryptoFairPicker.Drand;
using Moq;
using Xunit;

namespace CryptoFairPicker.Tests.Drand;

public class DrandWinnerSelectorTests
{
    [Fact]
    public async Task PickWinnerAsync_ReturnsValueInCorrectRange()
    {
        // Arrange
        var mockSource = new Mock<IFairRandomSource>();
        mockSource
            .Setup(s => s.NextIntAsync(It.IsAny<int>(), It.IsAny<RoundId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        
        var selector = new DrandWinnerSelector(mockSource.Object);
        var round = RoundId.FromRound(1000);

        // Act
        var winner = await selector.PickWinnerAsync(10, round);

        // Assert
        Assert.Equal(6, winner); // 5 + 1 = 6 (1-indexed)
        mockSource.Verify(s => s.NextIntAsync(10, round, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void PickWinner_ReturnsValueInCorrectRange()
    {
        // Arrange
        var mockSource = new Mock<IFairRandomSource>();
        mockSource
            .Setup(s => s.NextInt(It.IsAny<int>(), It.IsAny<RoundId>()))
            .Returns(3);
        
        var selector = new DrandWinnerSelector(mockSource.Object);
        var round = RoundId.FromRound(1000);

        // Act
        var winner = selector.PickWinner(10, round);

        // Assert
        Assert.Equal(4, winner); // 3 + 1 = 4 (1-indexed)
        mockSource.Verify(s => s.NextInt(10, round), Times.Once);
    }

    [Fact]
    public async Task PickWinnerAsync_Returns1ForFirstIndex()
    {
        // Arrange
        var mockSource = new Mock<IFairRandomSource>();
        mockSource
            .Setup(s => s.NextIntAsync(It.IsAny<int>(), It.IsAny<RoundId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        
        var selector = new DrandWinnerSelector(mockSource.Object);
        var round = RoundId.FromRound(1000);

        // Act
        var winner = await selector.PickWinnerAsync(100, round);

        // Assert
        Assert.Equal(1, winner); // 0 + 1 = 1
    }

    [Fact]
    public async Task PickWinnerAsync_ReturnsNForLastIndex()
    {
        // Arrange
        var mockSource = new Mock<IFairRandomSource>();
        mockSource
            .Setup(s => s.NextIntAsync(It.IsAny<int>(), It.IsAny<RoundId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(99);
        
        var selector = new DrandWinnerSelector(mockSource.Object);
        var round = RoundId.FromRound(1000);

        // Act
        var winner = await selector.PickWinnerAsync(100, round);

        // Assert
        Assert.Equal(100, winner); // 99 + 1 = 100
    }

    [Fact]
    public async Task PickWinnerAsync_ThrowsForInvalidN()
    {
        // Arrange
        var mockSource = new Mock<IFairRandomSource>();
        var selector = new DrandWinnerSelector(mockSource.Object);
        var round = RoundId.FromRound(1000);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => selector.PickWinnerAsync(0, round));
        await Assert.ThrowsAsync<ArgumentException>(() => selector.PickWinnerAsync(-1, round));
    }

    [Fact]
    public async Task PickWinnerAsync_ThrowsForNullRound()
    {
        // Arrange
        var mockSource = new Mock<IFairRandomSource>();
        var selector = new DrandWinnerSelector(mockSource.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => selector.PickWinnerAsync(10, null!));
    }

    [Fact]
    public void Constructor_ThrowsForNullSource()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DrandWinnerSelector(null!));
    }
}
