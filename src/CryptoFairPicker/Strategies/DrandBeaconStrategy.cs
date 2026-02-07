using System.Security.Cryptography;
using System.Text.Json;

namespace CryptoFairPicker.Strategies;

/// <summary>
/// A strategy that uses external randomness beacons like drand for verifiable randomness.
/// drand provides public, verifiable randomness from a distributed network.
/// </summary>
/// <remarks>
/// Initializes a new instance of DrandBeaconStrategy.
/// </remarks>
/// <param name="httpClient">HttpClient for making requests to the beacon.</param>
/// <param name="beaconUrl">Optional custom beacon URL. Defaults to drand quicknet.</param>
public class DrandBeaconStrategy(HttpClient httpClient, string beaconUrl) : IPickerStrategy
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly string _beaconUrl = beaconUrl;

    /// <inheritdoc />
    public int Pick(int optionCount)
    {
        // For synchronous version, we'll use Task.Run to execute the async method
        return PickAsync(optionCount).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<int> PickAsync(int optionCount, CancellationToken cancellationToken = default)
    {
        if (optionCount <= 0)
        {
            throw new ArgumentException("Option count must be greater than zero.", nameof(optionCount));
        }

        // Fetch the latest randomness from drand
        var randomness = await FetchRandomnessAsync(cancellationToken);

        // Derive a pick from the randomness
        return DerivePickFromRandomness(randomness, optionCount);
    }

    /// <summary>
    /// Fetches the latest randomness from the drand beacon.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The randomness bytes.</returns>
    public async Task<byte[]> FetchRandomnessAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetStringAsync(_beaconUrl, cancellationToken);
        
        using var doc = JsonDocument.Parse(response);
        var root = doc.RootElement;
        
        // The randomness field contains hex-encoded random bytes
        var randomnessHex = root.GetProperty("randomness").GetString();
        
        if (string.IsNullOrEmpty(randomnessHex))
        {
            throw new InvalidOperationException("Failed to retrieve randomness from beacon.");
        }

        return Convert.FromHexString(randomnessHex);
    }

    /// <summary>
    /// Derives a pick index from the randomness bytes.
    /// </summary>
    /// <param name="randomness">The randomness bytes.</param>
    /// <param name="optionCount">The number of options.</param>
    /// <returns>The selected index.</returns>
    private static int DerivePickFromRandomness(byte[] randomness, int optionCount)
    {
        // Use the randomness as a seed for HMAC to generate uniform values
        using var hmac = new HMACSHA256(randomness);
        var counter = 0;
        
        while (true)
        {
            // Generate deterministic random bytes from the randomness
            var input = BitConverter.GetBytes(counter);
            var hash = hmac.ComputeHash(input);
            
            // Convert first 8 bytes to ulong
            var value = BitConverter.ToUInt64(hash, 0);
            
            // Calculate the maximum valid value to avoid bias
            var maxValue = ulong.MaxValue - (ulong.MaxValue % (ulong)optionCount);
            
            // Use rejection sampling for uniform distribution
            if (value < maxValue)
            {
                return (int)(value % (ulong)optionCount);
            }
            
            // Rejection sampling: try again with next counter value
            counter++;
            
            // Sanity check to prevent infinite loop (should never happen in practice)
            if (counter > 1000)
            {
                throw new InvalidOperationException("Failed to generate random number after 1000 attempts");
            }
        }
    }

    /// <summary>
    /// Gets information about a specific round from drand.
    /// </summary>
    /// <param name="round">The round number to fetch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response containing round information.</returns>
    public async Task<string> GetRoundInfoAsync(long round, CancellationToken cancellationToken = default)
    {
        var roundUrl = _beaconUrl.Replace("/latest", $"/{round}");
        return await _httpClient.GetStringAsync(roundUrl, cancellationToken);
    }
}
