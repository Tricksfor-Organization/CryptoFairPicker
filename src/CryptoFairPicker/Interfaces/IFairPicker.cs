namespace CryptoFairPicker.Interfaces;

/// <summary>
/// Represents a fair and cryptographically secure picker for selecting winners.
/// </summary>
public interface IFairPicker
{
    /// <summary>
    /// Picks a single winner index from 0 to optionCount-1.
    /// </summary>
    /// <param name="optionCount">The total number of options to pick from.</param>
    /// <returns>A zero-based index representing the winner.</returns>
    int PickWinner(int optionCount);

    /// <summary>
    /// Picks a single winner index from 0 to optionCount-1 asynchronously.
    /// </summary>
    /// <param name="optionCount">The total number of options to pick from.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A zero-based index representing the winner.</returns>
    Task<int> PickWinnerAsync(int optionCount, CancellationToken cancellationToken = default);
}
