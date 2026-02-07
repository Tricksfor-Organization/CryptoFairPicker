using CryptoFairPicker.Csprng;
using CryptoFairPicker.Drand;
using CryptoFairPicker.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoFairPicker.Extensions;

/// <summary>
/// Extension methods for registering drand and CSPRNG winner selection services with dependency injection.
/// </summary>
public static class WinnerSelectorServiceCollectionExtensions
{
    /// <summary>
    /// Adds CryptoFairPicker services with the drand randomness beacon strategy.
    /// This is the recommended default for verifiable public randomness.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action for drand options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCryptoFairPickerDrand(
        this IServiceCollection services,
        Action<DrandOptions>? configure = null)
    {
        // Configure options
        var optionsBuilder = services.AddOptions<DrandOptions>();
        if (configure != null)
        {
            optionsBuilder.Configure(configure);
        }

        // Register HttpClient for drand with typed client pattern
        services.AddHttpClient<DrandRandomSource>();

        // Register services
        services.AddSingleton<IFairRandomSource, DrandRandomSource>();
        services.AddSingleton<IWinnerSelector, DrandWinnerSelector>();

        return services;
    }

    /// <summary>
    /// Adds CryptoFairPicker services with the drand randomness beacon strategy,
    /// configured from the application's configuration system.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="sectionPath">The configuration section path. Default is "CryptoFairPicker:Drand".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCryptoFairPickerDrand(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionPath = "CryptoFairPicker:Drand")
    {
        // Bind configuration to DrandOptions
        services.Configure<DrandOptions>(configuration.GetSection(sectionPath));

        // Register HttpClient for drand
        services.AddHttpClient<DrandRandomSource>();

        // Register services
        services.AddSingleton<IFairRandomSource, DrandRandomSource>();
        services.AddSingleton<IWinnerSelector, DrandWinnerSelector>();

        return services;
    }

    /// <summary>
    /// Adds CryptoFairPicker services with the CSPRNG strategy for local randomness.
    /// This provides a fast fallback option when drand is not available or not desired.
    /// Note: CSPRNG does not use the RoundId for determinism.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCryptoFairPickerCsprng(this IServiceCollection services)
    {
        services.AddSingleton<IFairRandomSource, CsprngRandomSource>();
        services.AddSingleton<IWinnerSelector, CsprngWinnerSelector>();
        return services;
    }

    /// <summary>
    /// Adds CryptoFairPicker services with strategy selection based on configuration.
    /// Reads "CryptoFairPicker:Strategy" to decide between "drand" or "csprng".
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCryptoFairPicker(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var strategy = configuration.GetValue<string>("CryptoFairPicker:Strategy") ?? "drand";

        return strategy.ToLowerInvariant() switch
        {
            "drand" => services.AddCryptoFairPickerDrand(configuration),
            "csprng" => services.AddCryptoFairPickerCsprng(),
            _ => throw new InvalidOperationException($"Unknown strategy '{strategy}'. Valid options are 'drand' or 'csprng'.")
        };
    }
}
