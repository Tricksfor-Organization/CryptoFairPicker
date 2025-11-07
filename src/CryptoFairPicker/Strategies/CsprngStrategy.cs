using System.Security.Cryptography;

namespace CryptoFairPicker.Strategies;

/// <summary>
/// A cryptographically secure random number generator strategy using RandomNumberGenerator.GetInt32.
/// This strategy ensures uniform distribution without modulo bias.
/// </summary>
public class CsprngStrategy : IPickerStrategy
{
    /// <inheritdoc />
    public int Pick(int optionCount)
    {
        if (optionCount <= 0)
        {
            throw new ArgumentException("Option count must be greater than zero.", nameof(optionCount));
        }

        // RandomNumberGenerator.GetInt32 handles uniform distribution correctly
        // without modulo bias by using rejection sampling internally
        return RandomNumberGenerator.GetInt32(0, optionCount);
    }

    /// <inheritdoc />
    public Task<int> PickAsync(int optionCount, CancellationToken cancellationToken = default)
    {
        // RandomNumberGenerator.GetInt32 is synchronous, so we wrap it in a Task
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Pick(optionCount));
    }
}
