using System.Security.Cryptography;
using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Models;

namespace CryptoFairPicker.Csprng;

/// <summary>
/// Implementation of IFairRandomSource using cryptographically secure random number generator (CSPRNG).
/// This provides fast, local randomness without external dependencies, suitable as a fallback.
/// Note: This does not use the RoundId for determinism - each call produces different randomness.
/// </summary>
public class CsprngRandomSource : IFairRandomSource
{
    /// <inheritdoc />
    public int NextInt(int toExclusive, RoundId round)
    {
        if (toExclusive <= 0)
        {
            throw new ArgumentException("Upper bound must be positive.", nameof(toExclusive));
        }

        // CSPRNG is stateless and doesn't use the round ID
        // This provides fresh randomness on each call
        return RandomNumberGenerator.GetInt32(0, toExclusive);
    }

    /// <inheritdoc />
    public Task<int> NextIntAsync(int toExclusive, RoundId round, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(NextInt(toExclusive, round));
    }
}
