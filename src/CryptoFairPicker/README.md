# Tricksfor.CryptoFairPicker

**Provably fair, cryptographically secure winner selection using public randomness beacons.**

A .NET library for transparent, verifiable, and unbiased winner selection. Perfect for lotteries, raffles, games, and any scenario where fairness matters.

## Features

✅ **Public Verifiability** - Uses [drand](https://drand.love/) public randomness beacon  
✅ **Deterministic** - Same round + participants = same winner, always  
✅ **Cryptographically Secure** - SHA-256 hashing with rejection sampling  
✅ **Pre-announcement Support** - Commit to future rounds before they're published  
✅ **Zero Trust Required** - External randomness eliminates "rigged draw" concerns  
✅ **Production Ready** - Retry logic, timeouts, comprehensive error handling  

## Quick Start

### Using Drand (Recommended)

```csharp
using CryptoFairPicker.Extensions;
using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Models;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddCryptoFairPickerDrand();
var provider = services.BuildServiceProvider();

var selector = provider.GetRequiredService<IWinnerSelector>();
var round = RoundId.FromRound(9000000);
var winner = await selector.PickWinnerAsync(100, round);

Console.WriteLine($"Winner: Participant #{winner}");
```

### Using Local CSPRNG (Fallback)

```csharp
using CryptoFairPicker.Extensions;
using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Models;

services.AddCryptoFairPickerCsprng();
var selector = provider.GetRequiredService<IWinnerSelector>();
var round = RoundId.FromRound(1); // Ignored for CSPRNG
var winner = await selector.PickWinnerAsync(100, round);
```

### Configuration

```json
{
  "CryptoFairPicker": {
    "Strategy": "drand",
    "Drand": {
      "BaseUrl": "https://api.drand.sh/public",
      "TimeoutSeconds": 10,
      "RetryCount": 3
    }
  }
}
```

```csharp
using CryptoFairPicker.Extensions;

builder.Services.AddCryptoFairPicker(builder.Configuration);
```

## How It Works

1. **Fetch** randomness from drand's public beacon for a specific round
2. **Hash** using SHA-256 to derive secure entropy
3. **Map** to range [1, n] using rejection sampling (no modulo bias)
4. **Return** deterministic winner

Anyone can verify the selection by fetching the same round from drand and reproducing the calculation.

## Core APIs

### IWinnerSelector (Recommended)
```csharp
using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Models;

int PickWinner(int n, RoundId round);
Task<int> PickWinnerAsync(int n, RoundId round, CancellationToken ct = default);
// Returns: Winner in range [1, n] (1-indexed)
// With 10 participants, possible results are 1, 2, … 10
```

### RoundId Helpers
```csharp
using CryptoFairPicker.Models;

// Convert a time to the corresponding drand quicknet round
var round = RoundId.FromTime(DateTimeOffset.UtcNow.AddHours(1));

// Get the estimated publication time for a round
var publishTime = round.GetEstimatedTime();
```

### IFairRandomSource (Lower-level)
```csharp
using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Models;

int NextInt(int toExclusive, RoundId round);
Task<int> NextIntAsync(int toExclusive, RoundId round, CancellationToken ct = default);
// Returns: Random value in range [0, toExclusive) (0-indexed)
```

## Pre-announced Draws

```csharp
using CryptoFairPicker.Interfaces;
using CryptoFairPicker.Models;

// Calculate the round for a future draw time
var drawTime = DateTimeOffset.UtcNow.AddHours(1);
var round = RoundId.FromTime(drawTime);

// Announce publicly
var publishTime = round.GetEstimatedTime();
Console.WriteLine($"Draw will use drand round {round.Value}");
Console.WriteLine($"Round published at: {publishTime:O}");
Console.WriteLine($"Verify: https://api.drand.sh/public/.../{round.Value}");

// Wait for round to be published...
var winner = await selector.PickWinnerAsync(100, round);
```

## Testing

The library includes comprehensive tests with NUnit and NSubstitute:
- Determinism validation
- Uniform distribution checks  
- Error handling for network failures
- Mock HTTP handlers for isolated testing

```bash
dotnet test
```

## Documentation

📚 [Full Documentation](https://github.com/Tricksfor-Organization/CryptoFairPicker)  
🔍 [Verification Guide](https://github.com/Tricksfor-Organization/CryptoFairPicker/blob/main/docs/VERIFY.md)  
💡 [Complete Examples](https://github.com/Tricksfor-Organization/CryptoFairPicker/tree/main/samples)  

## Requirements

- .NET 9.0 or later
- Internet access (for drand strategy)

## License

MIT License - See [LICENSE](https://github.com/Tricksfor-Organization/CryptoFairPicker/blob/main/LICENSE)

## Support

🐛 [Report Issues](https://github.com/Tricksfor-Organization/CryptoFairPicker/issues)  
💬 [Discussions](https://github.com/Tricksfor-Organization/CryptoFairPicker/discussions)
