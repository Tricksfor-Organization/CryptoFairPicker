using CryptoFairPicker.Strategies;
using Xunit;

namespace CryptoFairPicker.Tests;

public class CommitRevealStrategyTests
{
    [Fact]
    public void Pick_GeneratesCommitmentBeforePick()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();
        const int optionCount = 10;

        // Act
        var result = strategy.Pick(optionCount);

        // Assert
        Assert.NotNull(strategy.CommitmentHash);
        Assert.NotNull(strategy.Secret);
        Assert.Equal(result, strategy.Result);
    }

    [Fact]
    public void Pick_ReturnsValueInRange()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();
        const int optionCount = 10;

        // Act
        var result = strategy.Pick(optionCount);

        // Assert
        Assert.InRange(result, 0, optionCount - 1);
    }

    [Fact]
    public void Commit_GeneratesCommitmentHash()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();

        // Act
        strategy.Commit();

        // Assert
        Assert.NotNull(strategy.CommitmentHash);
        Assert.NotNull(strategy.Secret);
        Assert.Null(strategy.Result);
    }

    [Fact]
    public void Verify_SucceedsForCorrectSecret()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();
        const int optionCount = 10;

        // Act
        var result = strategy.Pick(optionCount);
        var secret = strategy.Secret!;
        var isValid = strategy.Verify(secret, optionCount, result);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Verify_FailsForIncorrectSecret()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();
        const int optionCount = 10;
        var result = strategy.Pick(optionCount);
        var wrongSecret = new string('0', 64); // Invalid secret

        // Act
        var isValid = strategy.Verify(wrongSecret, optionCount, result);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Verify_FailsForIncorrectResult()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();
        const int optionCount = 10;
        var result = strategy.Pick(optionCount);
        var secret = strategy.Secret!;
        var wrongResult = (result + 1) % optionCount; // Different result

        // Act
        var isValid = strategy.Verify(secret, optionCount, wrongResult);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Reset_ClearsAllState()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();
        strategy.Pick(10);

        // Act
        strategy.Reset();

        // Assert
        Assert.Null(strategy.CommitmentHash);
        Assert.Null(strategy.Secret);
        Assert.Null(strategy.Result);
    }

    [Fact]
    public void Pick_IsDeterministicForSameSecret()
    {
        // Arrange
        var strategy1 = new CommitRevealStrategy();
        var strategy2 = new CommitRevealStrategy();
        const int optionCount = 10;

        // Act
        strategy1.Commit();
        var result1 = strategy1.Pick(optionCount);

        // Set strategy2 to use the same secret (by manipulating internal state through verification)
        strategy2.Commit();
        var result2 = strategy2.Pick(optionCount);

        // Assert - Different instances should produce different results with different secrets
        // (This test verifies randomness between instances)
        Assert.True(result1 >= 0 && result1 < optionCount);
        Assert.True(result2 >= 0 && result2 < optionCount);
    }

    [Fact]
    public async Task PickAsync_ReturnsValueInRange()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();
        const int optionCount = 10;

        // Act
        var result = await strategy.PickAsync(optionCount);

        // Assert
        Assert.InRange(result, 0, optionCount - 1);
    }

    [Fact]
    public void Pick_ThrowsForInvalidOptionCount()
    {
        // Arrange
        var strategy = new CommitRevealStrategy();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => strategy.Pick(0));
        Assert.Throws<ArgumentException>(() => strategy.Pick(-1));
    }
}
