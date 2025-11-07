namespace CryptoFairPicker;

/// <summary>
/// Strategy interface for different picking algorithms.
/// </summary>
public interface IPickerStrategy
{
    /// <summary>
    /// Picks a single winner index from 0 to optionCount-1.
    /// </summary>
    /// <param name="optionCount">The total number of options to pick from.</param>
    /// <returns>A zero-based index representing the winner.</returns>
    int Pick(int optionCount);

    /// <summary>
    /// Picks a single winner index from 0 to optionCount-1 asynchronously.
    /// </summary>
    /// <param name="optionCount">The total number of options to pick from.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A zero-based index representing the winner.</returns>
    Task<int> PickAsync(int optionCount, CancellationToken cancellationToken = default);
}
