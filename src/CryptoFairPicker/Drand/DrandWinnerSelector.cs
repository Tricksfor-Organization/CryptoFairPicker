using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Services;

namespace CryptoFairPicker.Drand;

/// <summary>
/// Implementation of IWinnerSelector using drand randomness beacon.
/// Selects winners in the range [1, n] using verifiable public randomness.
/// </summary>
/// <remarks>
/// Initializes a new instance of DrandWinnerSelector.
/// </remarks>
/// <param name="randomSource">The fair random source to use for selection.</param>
public class DrandWinnerSelector(IFairRandomSource randomSource) : WinnerSelectorBase(randomSource)
{
}
