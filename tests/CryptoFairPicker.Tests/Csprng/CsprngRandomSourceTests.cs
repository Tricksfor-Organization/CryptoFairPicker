using CryptoFairPicker.Csprng;
using Xunit;

namespace CryptoFairPicker.Tests.Csprng;

public class CsprngRandomSourceTests
{
    [Fact]
    public void NextInt_ReturnsValueInRange()
    {
        // Arrange
        var source = new CsprngRandomSource();
        var round = RoundId.FromRound(1000);

        // Act
        var result = source.NextInt(10, round);

        // Assert
        Assert.InRange(result, 0, 9);
    }

    [Fact]
    public async Task NextIntAsync_ReturnsValueInRange()
    {
        // Arrange
        var source = new CsprngRandomSource();
        var round = RoundId.FromRound(1000);

        // Act
        var result = await source.NextIntAsync(10, round);

        // Assert
        Assert.InRange(result, 0, 9);
    }

    [Fact]
    public void NextInt_ThrowsForInvalidBound()
    {
        // Arrange
        var source = new CsprngRandomSource();
        var round = RoundId.FromRound(1000);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => source.NextInt(0, round));
        Assert.Throws<ArgumentException>(() => source.NextInt(-1, round));
    }

    [Fact]
    public void NextInt_ProducesVariedResults()
    {
        // Arrange
        var source = new CsprngRandomSource();
        var round = RoundId.FromRound(1000);
        var results = new HashSet<int>();

        // Act - Generate many values
        for (int i = 0; i < 100; i++)
        {
            results.Add(source.NextInt(100, round));
        }

        // Assert - Should get varied results (not all the same)
        Assert.True(results.Count > 50, "CSPRNG should produce varied results");
    }

    [Fact]
    public void NextInt_IsNotDeterministic()
    {
        // Arrange - CSPRNG does NOT use round for determinism
        var source = new CsprngRandomSource();
        var round = RoundId.FromRound(1000);

        // Act - Call with same round multiple times
        var results = new HashSet<int>();
        for (int i = 0; i < 20; i++)
        {
            results.Add(source.NextInt(100, round));
        }

        // Assert - Should get different results even with same round
        Assert.True(results.Count > 10, "CSPRNG should produce non-deterministic results");
    }
}
