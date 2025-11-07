# GitHub Copilot Instructions for CryptoFairPicker

## Project Overview

CryptoFairPicker is a .NET 9.0 library providing cryptographically secure, verifiable winner selection using public randomness beacons (primarily drand). The library emphasizes transparency, determinism, and verifiability.

## Architecture Principles

### Core Abstractions

1. **RoundId**: Immutable record identifying a specific randomness round
2. **IFairRandomSource**: Interface for obtaining random values for a specific round (0-indexed)
3. **IWinnerSelector**: Interface for selecting winners (1-indexed, range [1, n])

### Implementations

- **DrandRandomSource**: Fetches randomness from drand HTTP API, applies SHA-256 hashing
- **DrandWinnerSelector**: Wraps IFairRandomSource, adds 1 to convert to 1-indexed
- **CsprngRandomSource**: Local CSPRNG fallback (ignores RoundId, provides fresh randomness)
- **CsprngWinnerSelector**: Wraps CsprngRandomSource for local selection

### Key Design Decisions

1. **Winners are 1-indexed**: `PickWinner(n, round)` returns values in [1, n] for human-friendly participant numbering
2. **Random sources are 0-indexed**: `NextInt(n, round)` returns values in [0, n-1] for programming convenience
3. **Determinism**: Same RoundId + n always produces same result (for drand; CSPRNG is intentionally non-deterministic)
4. **Rejection sampling**: Used throughout to avoid modulo bias

## Code Style Guidelines

### Naming Conventions

- Interfaces: `IWinnerSelector`, `IFairRandomSource`
- Implementations: `DrandWinnerSelector`, `CsprngRandomSource`
- Configuration: `DrandOptions`, `WinnerSelectorServiceCollectionExtensions`
- Namespaces: `CryptoFairPicker`, `CryptoFairPicker.Drand`, `CryptoFairPicker.Csprng`

### Pattern Usage

```csharp
// ✅ GOOD: Dependency injection with options pattern
public DrandRandomSource(HttpClient httpClient, IOptions<DrandOptions> options)
{
    _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
}

// ✅ GOOD: Async with proper cancellation
public async Task<int> NextIntAsync(int toExclusive, RoundId round, CancellationToken cancellationToken = default)
{
    // Implementation
}

// ✅ GOOD: Rejection sampling for uniform distribution
var maxValue = ulong.MaxValue - (ulong.MaxValue % (ulong)n);
if (value < maxValue)
{
    return (int)(value % (ulong)n);
}

// ❌ BAD: Modulo bias
return (int)(randomValue % n);

// ❌ BAD: Ignoring cancellation token
public Task<int> DoSomethingAsync(CancellationToken token)
{
    return Task.FromResult(42); // Should check token
}
```

### Error Handling

```csharp
// ✅ GOOD: Descriptive error messages
if (toExclusive <= 0)
{
    throw new ArgumentException("Upper bound must be positive.", nameof(toExclusive));
}

// ✅ GOOD: Wrap external errors with context
catch (HttpRequestException ex)
{
    throw new InvalidOperationException(
        $"Failed to fetch randomness from drand beacon at {url}. " +
        "Ensure the round exists and the beacon is accessible.", ex);
}

// ❌ BAD: Generic errors
throw new Exception("Error occurred");
```

## API Design

### Public vs Internal

- **Public**: Interfaces (IWinnerSelector, IFairRandomSource), RoundId, DrandOptions, extension methods
- **Internal/Private**: Implementation details like MapToRange, DeriveRandomBlock

### XML Documentation

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Picks a winner from 1 to n (inclusive) for the given round.
/// </summary>
/// <param name="n">The number of participants (must be positive).</param>
/// <param name="round">The round identifier for deterministic selection.</param>
/// <returns>A winner number in [1, n].</returns>
int PickWinner(int n, RoundId round);
```

## Testing Guidelines

### Test Organization

- **Unit tests**: One test class per implementation (e.g., `DrandRandomSourceTests`)
- **Integration tests**: Optional, marked with `[Fact(Skip = "Integration test")]` by default
- **Mocking**: Use Moq for HTTP handlers, IFairRandomSource, etc.

### Test Patterns

```csharp
// ✅ GOOD: Descriptive test names
[Fact]
public async Task NextIntAsync_IsDeterministicForSameRound()

// ✅ GOOD: Arrange-Act-Assert pattern
[Fact]
public async Task PickWinnerAsync_ReturnsValueInCorrectRange()
{
    // Arrange
    var mockSource = CreateMockRandomSource();
    var selector = new DrandWinnerSelector(mockSource);
    var round = RoundId.FromRound(1000);

    // Act
    var winner = await selector.PickWinnerAsync(10, round);

    // Assert
    Assert.InRange(winner, 1, 10);
}

// ✅ GOOD: Test boundary conditions
[Fact]
public void PickWinner_ThrowsForInvalidN()
{
    Assert.Throws<ArgumentException>(() => selector.PickWinner(0, round));
    Assert.Throws<ArgumentException>(() => selector.PickWinner(-1, round));
}
```

## Configuration & DI

### Extension Methods

```csharp
// ✅ GOOD: Fluent configuration API
services.AddCryptoFairPickerDrand(options =>
{
    options.BaseUrl = "https://custom.drand.sh";
    options.TimeoutSeconds = 15;
});

// ✅ GOOD: Configuration binding
services.AddCryptoFairPickerDrand(configuration);

// ✅ GOOD: Strategy pattern with configuration
services.AddCryptoFairPicker(configuration); // Reads Strategy from config
```

### Service Registration

```csharp
// ✅ GOOD: Register as singleton for DI
services.AddSingleton<IFairRandomSource, DrandRandomSource>();
services.AddSingleton<IWinnerSelector, DrandWinnerSelector>();

// ✅ GOOD: Use HttpClientFactory
services.AddHttpClient<DrandRandomSource>();
```

## Drand-Specific Knowledge

### Chain Information

- **Quicknet**: Chain hash `52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971`
- **Round period**: 3 seconds
- **Genesis**: Approximately round 7000000 at 2023-02-15 14:00 UTC

### API Endpoints

```
GET https://api.drand.sh/public/{chain}/info         - Chain metadata
GET https://api.drand.sh/public/{chain}/latest       - Latest round
GET https://api.drand.sh/public/{chain}/{round}      - Specific round
```

### Round Calculation

```csharp
// Current round (approximate)
var elapsed = DateTimeOffset.UtcNow - genesisTime;
var currentRound = genesisRound + (long)(elapsed.TotalSeconds / roundPeriod);
```

## Security Considerations

1. **Always use rejection sampling** to avoid modulo bias
2. **Apply SHA-256** to drand randomness for additional entropy derivation
3. **Use HMAC-SHA256** for deterministic expansion when needed
4. **Validate inputs** (n > 0, round not null)
5. **Handle network errors** gracefully with retries and timeouts
6. **Never expose secrets** in logs or error messages

## Common Patterns

### Implementing IFairRandomSource

```csharp
public class MyRandomSource : IFairRandomSource
{
    public int NextInt(int toExclusive, RoundId round)
    {
        if (toExclusive <= 0)
            throw new ArgumentException("Upper bound must be positive.", nameof(toExclusive));
        if (round == null)
            throw new ArgumentNullException(nameof(round));
            
        // Fetch randomness for round
        var randomness = FetchRandomness(round);
        
        // Map to [0, toExclusive) using rejection sampling
        return MapToRange(randomness, toExclusive);
    }
    
    public async Task<int> NextIntAsync(int toExclusive, RoundId round, 
        CancellationToken cancellationToken = default)
    {
        // Similar implementation with async operations
    }
}
```

### Implementing IWinnerSelector

```csharp
public class MyWinnerSelector : IWinnerSelector
{
    private readonly IFairRandomSource _randomSource;
    
    public MyWinnerSelector(IFairRandomSource randomSource)
    {
        _randomSource = randomSource ?? throw new ArgumentNullException(nameof(randomSource));
    }
    
    public int PickWinner(int n, RoundId round)
    {
        if (n <= 0)
            throw new ArgumentException("Number of participants must be positive.", nameof(n));
        
        // Get 0-indexed random, convert to 1-indexed winner
        return _randomSource.NextInt(n, round) + 1;
    }
}
```

## Migration from Old API

The old `IPickerStrategy` API remains for backward compatibility:

- Old: `IPickerStrategy.Pick(int optionCount)` returns [0, optionCount-1]
- New: `IWinnerSelector.PickWinner(int n, RoundId round)` returns [1, n]

When working with both:
- Keep old implementations unchanged
- New implementations should focus on round-based determinism
- Document which API a class implements

## Additional Resources

- [Drand Documentation](https://drand.love/)
- [docs/VERIFY.md](docs/VERIFY.md) - Verification guide
- [samples/CryptoFairPicker.Sample](samples/CryptoFairPicker.Sample) - Example usage
- [CHANGELOG.md](CHANGELOG.md) - Version history
