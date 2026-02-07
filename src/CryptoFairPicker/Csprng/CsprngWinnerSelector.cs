using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Services;

namespace CryptoFairPicker.Csprng;

/// <summary>
/// Implementation of IWinnerSelector using CSPRNG for local randomness.
/// Provides a fast fallback option when drand is not available.
/// </summary>
/// <remarks>
/// Initializes a new instance of CsprngWinnerSelector.
/// </remarks>
/// <param name="randomSource">The fair random source to use for selection.</param>
public class CsprngWinnerSelector(IFairRandomSource randomSource) : WinnerSelectorBase(randomSource)
{
    /// <summary>
    /// Initializes a new instance of CsprngWinnerSelector with default CSPRNG source.
    /// </summary>
    public CsprngWinnerSelector() : this(new CsprngRandomSource())
    {
    }
}
