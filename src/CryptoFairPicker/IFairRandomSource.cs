namespace CryptoFairPicker;

/// <summary>
/// Represents a source of fair, verifiable randomness for a specific round.
/// </summary>
public interface IFairRandomSource
{
    /// <summary>
    /// Generates a random integer in the range [0, toExclusive) for the given round.
    /// </summary>
    /// <param name="toExclusive">The exclusive upper bound (must be positive).</param>
    /// <param name="round">The round identifier for deterministic randomness.</param>
    /// <returns>A random integer in [0, toExclusive).</returns>
    int NextInt(int toExclusive, RoundId round);

    /// <summary>
    /// Generates a random integer in the range [0, toExclusive) for the given round asynchronously.
    /// </summary>
    /// <param name="toExclusive">The exclusive upper bound (must be positive).</param>
    /// <param name="round">The round identifier for deterministic randomness.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A random integer in [0, toExclusive).</returns>
    Task<int> NextIntAsync(int toExclusive, RoundId round, CancellationToken cancellationToken = default);
}
