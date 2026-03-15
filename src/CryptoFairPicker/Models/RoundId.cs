namespace CryptoFairPicker.Models;

/// <summary>
/// Represents a unique identifier for a randomness round from a beacon like drand.
/// This can be a round number, timestamp, or other identifier depending on the beacon.
/// </summary>
/// <param name="Value">The round identifier value.</param>
public record RoundId(string Value)
{
    /// <summary>
    /// Drand quicknet genesis time (Unix seconds): 2023-08-23T15:29:27Z.
    /// </summary>
    private static readonly DateTimeOffset QuicknetGenesisTime =
        DateTimeOffset.FromUnixTimeSeconds(1692803367);

    /// <summary>
    /// Drand quicknet round period in seconds.
    /// </summary>
    private const int QuicknetPeriodSeconds = 3;

    /// <summary>
    /// Creates a RoundId from a long round number.
    /// </summary>
    /// <param name="roundNumber">The round number.</param>
    /// <returns>A RoundId instance.</returns>
    public static RoundId FromRound(long roundNumber) => new(roundNumber.ToString());

    /// <summary>
    /// Calculates the drand quicknet round number for the given point in time.
    /// Uses the permanent quicknet chain parameters (genesis 2023-08-23T15:29:27Z, 3-second period).
    /// </summary>
    /// <param name="time">The point in time to convert to a round number.</param>
    /// <returns>A RoundId for the round active at <paramref name="time"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="time"/> is before the quicknet genesis time.
    /// </exception>
    public static RoundId FromTime(DateTimeOffset time)
    {
        if (time < QuicknetGenesisTime)
        {
            throw new ArgumentOutOfRangeException(
                nameof(time),
                time,
                $"Time must not be before the drand quicknet genesis ({QuicknetGenesisTime:O}).");
        }

        var elapsedSeconds = (long)(time - QuicknetGenesisTime).TotalSeconds;
        var round = (elapsedSeconds / QuicknetPeriodSeconds) + 1;
        return FromRound(round);
    }

    /// <summary>
    /// Estimates the time at which the drand quicknet round identified by this RoundId will be published.
    /// Uses the permanent quicknet chain parameters (genesis 2023-08-23T15:29:27Z, 3-second period).
    /// </summary>
    /// <returns>The estimated publication time for this round.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the Value cannot be parsed as a round number or the round number is less than 1.
    /// </exception>
    public DateTimeOffset GetEstimatedTime()
    {
        if (!TryGetRoundNumber(out var round) || round < 1)
        {
            throw new InvalidOperationException(
                $"Cannot estimate time: RoundId value '{Value}' is not a valid positive round number.");
        }

        return QuicknetGenesisTime.AddSeconds((round - 1) * QuicknetPeriodSeconds);
    }

    /// <summary>
    /// Tries to parse the RoundId value as a long.
    /// </summary>
    /// <param name="result">The parsed round number if successful.</param>
    /// <returns>True if parsing succeeded, false otherwise.</returns>
    public bool TryGetRoundNumber(out long result) => long.TryParse(Value, out result);

    /// <summary>
    /// Gets the round number as a long, throwing if the value cannot be parsed.
    /// </summary>
    /// <returns>The round number.</returns>
    /// <exception cref="FormatException">Thrown when the Value is not a valid long.</exception>
    public long GetRoundNumber() => long.Parse(Value);
}
