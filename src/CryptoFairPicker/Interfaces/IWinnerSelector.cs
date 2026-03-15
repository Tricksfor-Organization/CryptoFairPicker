using CryptoFairPicker.Models;

namespace CryptoFairPicker.Interfaces;

/// <summary>
/// Represents a service that selects winners fairly using verifiable randomness.
/// Winners are 1-indexed: results are always in the range [1, n] for human-friendly participant numbering.
/// For example, <c>PickWinner(10, round)</c> returns an integer from 1 to 10 inclusive.
/// </summary>
public interface IWinnerSelector
{
    /// <summary>
    /// Picks a winner from 1 to <paramref name="n"/> (inclusive) for the given round.
    /// The result is 1-indexed: with 10 participants, possible results are 1, 2, … 10.
    /// </summary>
    /// <param name="n">The number of participants (must be positive).</param>
    /// <param name="round">The round identifier for deterministic selection.</param>
    /// <returns>A 1-indexed winner number in the range [1, <paramref name="n"/>].</returns>
    int PickWinner(int n, RoundId round);

    /// <summary>
    /// Picks a winner from 1 to <paramref name="n"/> (inclusive) for the given round asynchronously.
    /// The result is 1-indexed: with 10 participants, possible results are 1, 2, … 10.
    /// </summary>
    /// <param name="n">The number of participants (must be positive).</param>
    /// <param name="round">The round identifier for deterministic selection.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A 1-indexed winner number in the range [1, <paramref name="n"/>].</returns>
    Task<int> PickWinnerAsync(int n, RoundId round, CancellationToken cancellationToken = default);
}
