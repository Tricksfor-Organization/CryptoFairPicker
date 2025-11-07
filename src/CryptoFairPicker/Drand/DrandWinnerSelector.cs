namespace CryptoFairPicker.Drand;

/// <summary>
/// Implementation of IWinnerSelector using drand randomness beacon.
/// Selects winners in the range [1, n] using verifiable public randomness.
/// </summary>
public class DrandWinnerSelector : IWinnerSelector
{
    private readonly IFairRandomSource _randomSource;

    /// <summary>
    /// Initializes a new instance of DrandWinnerSelector.
    /// </summary>
    /// <param name="randomSource">The fair random source to use for selection.</param>
    public DrandWinnerSelector(IFairRandomSource randomSource)
    {
        _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
    }

    /// <inheritdoc />
    public int PickWinner(int n, RoundId round)
    {
        if (n <= 0)
        {
            throw new ArgumentException("Number of participants must be positive.", nameof(n));
        }

        if (round == null)
        {
            throw new ArgumentNullException(nameof(round));
        }

        // Get random value in [0, n) and add 1 to get [1, n]
        return _randomSource.NextInt(n, round) + 1;
    }

    /// <inheritdoc />
    public async Task<int> PickWinnerAsync(int n, RoundId round, CancellationToken cancellationToken = default)
    {
        if (n <= 0)
        {
            throw new ArgumentException("Number of participants must be positive.", nameof(n));
        }

        if (round == null)
        {
            throw new ArgumentNullException(nameof(round));
        }

        // Get random value in [0, n) and add 1 to get [1, n]
        var winner = await _randomSource.NextIntAsync(n, round, cancellationToken);
        return winner + 1;
    }
}
