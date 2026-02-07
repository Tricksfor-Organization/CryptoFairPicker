using System.Security.Cryptography;
using System.Text.Json;
using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Models;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace CryptoFairPicker.Drand;

/// <summary>
/// Implementation of IFairRandomSource using the drand public randomness beacon.
/// Fetches verifiable randomness from drand HTTP API and derives uniform random values.
/// </summary>
public class DrandRandomSource : IFairRandomSource
{
    private readonly HttpClient _httpClient;
    private readonly DrandOptions _options;
    private readonly AsyncRetryPolicy _retryPolicy;

    private const int MaxRejectionSamplingAttempts = 1000;

    /// <summary>
    /// Initializes a new instance of DrandRandomSource.
    /// </summary>
    /// <param name="httpClient">HTTP client for making requests to drand.</param>
    /// <param name="options">Configuration options for drand integration.</param>
    public DrandRandomSource(HttpClient httpClient, IOptions<DrandOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        
        // Configure timeout
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        // Configure retry policy for transient failures
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: _options.RetryCount,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    // Optional: Log retry attempts
                });
    }

    /// <inheritdoc />
    public int NextInt(int toExclusive, RoundId round)
    {
        // Note: Using GetAwaiter().GetResult() is acceptable here as this is a library method
        // that may be called from synchronous contexts. Users should prefer NextIntAsync when possible.
        return NextIntAsync(toExclusive, round).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async Task<int> NextIntAsync(int toExclusive, RoundId round, CancellationToken cancellationToken = default)
    {
        if (toExclusive <= 0)
        {
            throw new ArgumentException("Upper bound must be positive.", nameof(toExclusive));
        }

        if (round == null)
        {
            throw new ArgumentNullException(nameof(round));
        }

        // Fetch randomness for the specified round
        var randomness = await FetchRandomnessAsync(round, cancellationToken);

        // Derive a 32-byte block using SHA-256
        var derivedBlock = DeriveRandomBlock(randomness);

        // Map uniformly to [0, toExclusive) using rejection sampling
        return MapToRange(derivedBlock, toExclusive);
    }

    /// <summary>
    /// Fetches randomness from the drand beacon for a specific round.
    /// </summary>
    /// <param name="round">The round identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The randomness bytes from the beacon.</returns>
    private async Task<byte[]> FetchRandomnessAsync(RoundId round, CancellationToken cancellationToken)
    {
        var url = $"{_options.GetBeaconUrl()}/{round.Value}";

        try
        {
            var response = await _retryPolicy.ExecuteAsync(async () =>
            {
                var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
                httpResponse.EnsureSuccessStatusCode();
                return await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            });

            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;

            if (!root.TryGetProperty("randomness", out var randomnessElement))
            {
                throw new InvalidOperationException($"Response from drand does not contain 'randomness' field. URL: {url}");
            }

            var randomnessHex = randomnessElement.GetString();
            if (string.IsNullOrEmpty(randomnessHex))
            {
                throw new InvalidOperationException($"Randomness field is empty. URL: {url}");
            }

            return Convert.FromHexString(randomnessHex);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Failed to fetch randomness from drand beacon at {url}. Ensure the round exists and the beacon is accessible.", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON response from drand beacon at {url}.", ex);
        }
    }

    /// <summary>
    /// Derives a deterministic 32-byte random block from the beacon randomness using SHA-256.
    /// </summary>
    /// <param name="randomness">The randomness bytes from the beacon.</param>
    /// <returns>A 32-byte derived random block.</returns>
    private static byte[] DeriveRandomBlock(byte[] randomness)
    {
        return SHA256.HashData(randomness);
    }

    /// <summary>
    /// Maps a random byte array uniformly to [0, toExclusive) using rejection sampling to avoid modulo bias.
    /// </summary>
    /// <param name="randomBlock">The 32-byte random block.</param>
    /// <param name="toExclusive">The exclusive upper bound.</param>
    /// <returns>A random integer in [0, toExclusive).</returns>
    private static int MapToRange(byte[] randomBlock, int toExclusive)
    {
        // Use HMAC-SHA256 for deterministic expansion if needed
        using var hmac = new HMACSHA256(randomBlock);
        var counter = 0;

        while (true)
        {
            // Generate deterministic bytes from the random block
            var input = BitConverter.GetBytes(counter);
            var hash = hmac.ComputeHash(input);

            // Convert first 8 bytes to ulong
            var value = BitConverter.ToUInt64(hash, 0);

            // Calculate the maximum valid value to avoid modulo bias
            var maxValue = ulong.MaxValue - (ulong.MaxValue % (ulong)toExclusive);

            // Use rejection sampling for uniform distribution
            if (value < maxValue)
            {
                return (int)(value % (ulong)toExclusive);
            }

            // Rejection sampling: try again with next counter value
            counter++;

            // Sanity check to prevent infinite loop (should never happen in practice)
            if (counter > MaxRejectionSamplingAttempts)
            {
                throw new InvalidOperationException($"Failed to generate random number after {MaxRejectionSamplingAttempts} attempts using rejection sampling.");
            }
        }
    }

    /// <summary>
    /// Gets information about a specific round from drand (for debugging/verification).
    /// </summary>
    /// <param name="round">The round identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JSON response containing round information.</returns>
    public async Task<string> GetRoundInfoAsync(RoundId round, CancellationToken cancellationToken = default)
    {
        var url = $"{_options.GetBeaconUrl()}/{round.Value}";
        return await _httpClient.GetStringAsync(url, cancellationToken);
    }
}
