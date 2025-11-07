namespace CryptoFairPicker.Drand;

/// <summary>
/// Configuration options for drand randomness beacon integration.
/// </summary>
public class DrandOptions
{
    /// <summary>
    /// Gets or sets the base URL for the drand HTTP API.
    /// Default is the public drand quicknet endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.drand.sh/public";

    /// <summary>
    /// Gets or sets the chain hash identifier for the drand network.
    /// Default is quicknet (3-second rounds).
    /// Set to null to use the default chain from BaseUrl.
    /// </summary>
    public string? Chain { get; set; } = "52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971";

    /// <summary>
    /// Gets or sets the timeout for HTTP requests to the drand beacon (in seconds).
    /// Default is 10 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of retry attempts for transient network failures.
    /// Default is 3.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Gets the full beacon URL based on the configured BaseUrl and Chain.
    /// </summary>
    /// <returns>The full beacon URL.</returns>
    public string GetBeaconUrl()
    {
        if (string.IsNullOrEmpty(Chain))
        {
            return BaseUrl;
        }
        return $"{BaseUrl.TrimEnd('/')}/{Chain}";
    }
}
