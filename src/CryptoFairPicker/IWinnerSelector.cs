namespace CryptoFairPicker;

/// <summary>
/// Represents a service that selects winners fairly using verifiable randomness.
/// Winners are in the range [1, n] (1-indexed).
/// </summary>
public interface IWinnerSelector
{
    /// <summary>
    /// Picks a winner from 1 to n (inclusive) for the given round.
    /// </summary>
    /// <param name="n">The number of participants (must be positive).</param>
    /// <param name="round">The round identifier for deterministic selection.</param>
    /// <returns>A winner number in [1, n].</returns>
    int PickWinner(int n, RoundId round);

    /// <summary>
    /// Picks a winner from 1 to n (inclusive) for the given round asynchronously.
    /// </summary>
    /// <param name="n">The number of participants (must be positive).</param>
    /// <param name="round">The round identifier for deterministic selection.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A winner number in [1, n].</returns>
    Task<int> PickWinnerAsync(int n, RoundId round, CancellationToken cancellationToken = default);
}
