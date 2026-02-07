using CryptoFairPicker.Interfaces;

namespace CryptoFairPicker.Services;

/// <summary>
/// Default implementation of IFairPicker that delegates to a strategy.
/// </summary>
public class FairPicker : IFairPicker
{
    private readonly IPickerStrategy _strategy;

    /// <summary>
    /// Initializes a new instance of FairPicker with the specified strategy.
    /// </summary>
    /// <param name="strategy">The picking strategy to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when strategy is null.</exception>
    public FairPicker(IPickerStrategy strategy)
    {
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    }

    /// <inheritdoc />
    public int PickWinner(int optionCount)
    {
        if (optionCount <= 0)
        {
            throw new ArgumentException("Option count must be greater than zero.", nameof(optionCount));
        }

        return _strategy.Pick(optionCount);
    }

    /// <inheritdoc />
    public Task<int> PickWinnerAsync(int optionCount, CancellationToken cancellationToken = default)
    {
        if (optionCount <= 0)
        {
            throw new ArgumentException("Option count must be greater than zero.", nameof(optionCount));
        }

        return _strategy.PickAsync(optionCount, cancellationToken);
    }
}
