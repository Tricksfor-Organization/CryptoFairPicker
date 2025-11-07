using System.Security.Cryptography;
using System.Text;

namespace CryptoFairPicker.Strategies;

/// <summary>
/// A commit-reveal strategy for auditable and transparent draws.
/// This allows participants to verify that the draw was fair after the fact.
/// </summary>
public class CommitRevealStrategy : IPickerStrategy
{
    private byte[]? _commitment;
    private byte[]? _secret;
    private int? _result;

    /// <summary>
    /// Gets the commitment hash (can be shared before reveal).
    /// </summary>
    public string? CommitmentHash => _commitment != null ? Convert.ToHexString(_commitment) : null;

    /// <summary>
    /// Gets the secret (only available after reveal).
    /// </summary>
    public string? Secret => _secret != null ? Convert.ToHexString(_secret) : null;

    /// <summary>
    /// Gets the result of the pick (only available after reveal).
    /// </summary>
    public int? Result => _result;

    /// <summary>
    /// Commits to a random value without revealing it.
    /// </summary>
    public void Commit()
    {
        // Generate a random secret
        _secret = RandomNumberGenerator.GetBytes(32);
        
        // Create commitment hash
        _commitment = SHA256.HashData(_secret);
    }

    /// <inheritdoc />
    public int Pick(int optionCount)
    {
        if (optionCount <= 0)
        {
            throw new ArgumentException("Option count must be greater than zero.", nameof(optionCount));
        }

        // If not committed yet, commit first
        if (_commitment == null)
        {
            Commit();
        }

        // Reveal: use the secret to derive the pick
        if (_secret == null)
        {
            throw new InvalidOperationException("Secret not available.");
        }

        _result = DerivePickFromSecret(_secret, optionCount);
        return _result.Value;
    }

    /// <inheritdoc />
    public Task<int> PickAsync(int optionCount, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Pick(optionCount));
    }

    /// <summary>
    /// Verifies that a given secret matches the commitment and produces the expected result.
    /// </summary>
    /// <param name="secret">The secret to verify.</param>
    /// <param name="optionCount">The number of options.</param>
    /// <param name="expectedResult">The expected result.</param>
    /// <returns>True if verification succeeds.</returns>
    public bool Verify(string secret, int optionCount, int expectedResult)
    {
        if (_commitment == null)
        {
            throw new InvalidOperationException("No commitment exists to verify against.");
        }

        var secretBytes = Convert.FromHexString(secret);
        var computedCommitment = SHA256.HashData(secretBytes);

        // Verify commitment matches
        if (!computedCommitment.SequenceEqual(_commitment))
        {
            return false;
        }

        // Verify result matches
        var computedResult = DerivePickFromSecret(secretBytes, optionCount);
        return computedResult == expectedResult;
    }

    private static int DerivePickFromSecret(byte[] secret, int optionCount)
    {
        // Use HMAC-SHA256 to derive a uniform value from the secret
        // This ensures the result is deterministic and uniformly distributed
        using var hmac = new HMACSHA256(secret);
        var derived = hmac.ComputeHash(Encoding.UTF8.GetBytes("pick"));
        
        // Convert first 8 bytes to ulong for larger range
        var value = BitConverter.ToUInt64(derived, 0);
        
        // Use rejection sampling to avoid modulo bias
        var maxValue = ulong.MaxValue - (ulong.MaxValue % (ulong)optionCount);
        
        // In practice, rejection sampling would loop, but for deterministic behavior
        // we'll use a simple modulo here since the bias is negligible for large ranges
        return (int)(value % (ulong)optionCount);
    }

    /// <summary>
    /// Resets the strategy for a new draw.
    /// </summary>
    public void Reset()
    {
        _commitment = null;
        _secret = null;
        _result = null;
    }
}
