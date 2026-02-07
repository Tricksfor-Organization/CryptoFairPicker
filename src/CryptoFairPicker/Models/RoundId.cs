namespace CryptoFairPicker.Models;

/// <summary>
/// Represents a unique identifier for a randomness round from a beacon like drand.
/// This can be a round number, timestamp, or other identifier depending on the beacon.
/// </summary>
/// <param name="Value">The round identifier value.</param>
public record RoundId(string Value)
{
    /// <summary>
    /// Creates a RoundId from a long round number.
    /// </summary>
    /// <param name="roundNumber">The round number.</param>
    /// <returns>A RoundId instance.</returns>
    public static RoundId FromRound(long roundNumber) => new(roundNumber.ToString());

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
