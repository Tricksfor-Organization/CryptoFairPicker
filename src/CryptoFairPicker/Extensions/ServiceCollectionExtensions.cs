using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Services;
using CryptoFairPicker.Strategies;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoFairPicker.Extensions;

/// <summary>
/// Extension methods for registering CryptoFairPicker services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CryptoFairPicker services with the CSPRNG strategy.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCryptoFairPicker(this IServiceCollection services)
    {
        return AddCryptoFairPicker<CsprngStrategy>(services);
    }

    /// <summary>
    /// Adds CryptoFairPicker services with a custom strategy.
    /// </summary>
    /// <typeparam name="TStrategy">The strategy type to use.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCryptoFairPicker<TStrategy>(this IServiceCollection services)
        where TStrategy : class, IPickerStrategy
    {
        services.AddTransient<IPickerStrategy, TStrategy>();
        services.AddTransient<IFairPicker, FairPicker>();
        return services;
    }

    /// <summary>
    /// Adds CryptoFairPicker services with the Commit-Reveal strategy.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCommitRevealPicker(this IServiceCollection services)
    {
        services.AddSingleton<CommitRevealStrategy>();
        services.AddTransient<IPickerStrategy>(sp => sp.GetRequiredService<CommitRevealStrategy>());
        services.AddTransient<IFairPicker, FairPicker>();
        return services;
    }

    /// <summary>
    /// Adds CryptoFairPicker services with the Drand Beacon strategy.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="beaconUrl">Custom beacon URL.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDrandBeaconPicker(this IServiceCollection services, string beaconUrl)
    {
        services.AddHttpClient<DrandBeaconStrategy>();
        services.AddTransient<IPickerStrategy>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(nameof(DrandBeaconStrategy));
            return new DrandBeaconStrategy(httpClient, beaconUrl);
        });
        services.AddTransient<IFairPicker, FairPicker>();
        return services;
    }
}
