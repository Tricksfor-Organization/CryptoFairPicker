# CryptoFairPicker

**Provably fair, cryptographically secure winner selection using public randomness beacons.**

CryptoFairPicker is a .NET library that provides transparent, verifiable, and unbiased winner selection for lotteries, raffles, games, and any scenario where fairness matters. By default, it uses [drand](https://drand.love/) – a distributed public randomness beacon – enabling anyone to verify that selections are fair and unpredictable.

## 🎯 Why CryptoFairPicker?

- **Public Verifiability**: Uses drand's public randomness beacon – anyone can verify your draws
- **Deterministic & Reproducible**: Same round + participants = same winner, every time
- **Cryptographically Secure**: SHA-256 hashing with rejection sampling for uniform distribution
- **Pre-announcement Support**: Commit to a future round before it's published
- **Zero Trust Required**: External randomness source eliminates "rigged draw" concerns
- **Production Ready**: Includes retry logic, timeouts, and comprehensive error handling

## 📦 Installation

```bash
dotnet add package Tricksfor.CryptoFairPicker
```

Or add directly to your `.csproj`:

```xml
<PackageReference Include="Tricksfor.CryptoFairPicker" Version="9.*.*" />
```

## 🚀 Quick Start

### Using Drand (Recommended)

```csharp
using Tricksfor.CryptoFairPicker;
using Microsoft.Extensions.DependencyInjection;

// Setup DI with drand
var services = new ServiceCollection();
services.AddCryptoFairPickerDrand();
var provider = services.BuildServiceProvider();

// Get the winner selector
var selector = provider.GetRequiredService<IWinnerSelector>();

// Pick a winner for round 9000000 with 100 participants
var round = RoundId.FromRound(9000000);
var winner = await selector.PickWinnerAsync(100, round);

Console.WriteLine($"Winner: Participant #{winner}");
// Output: Winner: Participant #42 (example – actual result is deterministic)
```

### Configuration via appsettings.json

```json
{
  "CryptoFairPicker": {
    "Strategy": "drand",
    "Drand": {
      "BaseUrl": "https://api.drand.sh/public",
      "Chain": "52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971",
      "TimeoutSeconds": 10,
      "RetryCount": 3
    }
  }
}
```

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddCryptoFairPicker(builder.Configuration);
```

## 🎲 How It Works

### 1. Drand Randomness Beacon

[Drand](https://drand.love/) is a distributed network that produces verifiable, unpredictable, and bias-resistant randomness every 3 seconds. Each "round" contains:

- **Round number**: Sequential identifier
- **Randomness**: 32 bytes of unbiased random data (hex-encoded)
- **Signature**: BLS signature proving authenticity
- **Timestamp**: When the round was created

### 2. Deterministic Selection

CryptoFairPicker takes drand randomness and derives a winner:

1. **Fetch** randomness for a specific round from drand HTTP API
2. **Hash** the randomness using SHA-256 to derive a 32-byte block
3. **Map** to the desired range [1, n] using rejection sampling (avoids modulo bias)
4. **Return** the winner number

### 3. Verification

Anyone can verify the selection:

```bash
# Fetch the randomness for round 9000000
curl https://api.drand.sh/public/52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971/9000000

# Use CryptoFairPicker to reproduce the winner
dotnet run -- --round 9000000 --participants 100
```

See [docs/VERIFY.md](docs/VERIFY.md) for detailed verification steps.

## 📚 Core Concepts

### RoundId

Identifies a specific randomness round from drand:

```csharp
// From round number
var round = RoundId.FromRound(9000000);

// From string
var round = new RoundId("9000000");

// Parse round number
if (round.TryGetRoundNumber(out long roundNum))
{
    Console.WriteLine($"Round: {roundNum}");
}
```

### IWinnerSelector

Primary interface for selecting winners (1-indexed):

```csharp
public interface IWinnerSelector
{
    int PickWinner(int n, RoundId round);
    Task<int> PickWinnerAsync(int n, RoundId round, CancellationToken cancellationToken = default);
}
```

- `n`: Number of participants (must be positive)
- `round`: The randomness round to use
- Returns: Winner number in range **[1, n]** (1-indexed)

### IFairRandomSource

Lower-level interface for random number generation (0-indexed):

```csharp
public interface IFairRandomSource
{
    int NextInt(int toExclusive, RoundId round);
    Task<int> NextIntAsync(int toExclusive, RoundId round, CancellationToken cancellationToken = default);
}
```

## 🛠️ Strategies

### Drand Strategy (Default)

Uses public randomness from the drand network:

```csharp
services.AddCryptoFairPickerDrand(options =>
{
    options.BaseUrl = "https://api.drand.sh/public";
    options.Chain = "52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971";
    options.TimeoutSeconds = 10;
    options.RetryCount = 3;
});
```

**Benefits:**
- Publicly verifiable randomness
- No trust required in the operator
- Perfect for high-stakes or public draws
- Supports pre-announcement

**Considerations:**
- Requires internet access
- Depends on drand network availability
- 3-second round period (for quicknet chain)

### CSPRNG Strategy (Fallback)

Uses local cryptographically secure random number generator:

```csharp
services.AddCryptoFairPickerCsprng();
```

**Benefits:**
- Fast and local (no network required)
- Cryptographically secure
- No external dependencies

**Considerations:**
- Not deterministic (RoundId is ignored)
- Cannot be verified by third parties
- Trust required in the operator

## 🔧 Advanced Usage

### Pre-announced Draws

Announce the round before it's published for maximum transparency:

```csharp
// Calculate future round (approximately 1 hour from now)
var futureRound = CalculateFutureRound(hoursFromNow: 1);

// Announce publicly
Console.WriteLine($"The draw will use drand round {futureRound}");
Console.WriteLine($"Verify at: https://api.drand.sh/public/.../({futureRound}");

// Wait for the round to be published...
// (approximately 1 hour)

// Perform the draw
var winner = await selector.PickWinnerAsync(100, RoundId.FromRound(futureRound));
```

### Calculating Drand Rounds

```csharp
static long CalculateFutureRound(int hoursFromNow)
{
    // Drand quicknet genesis (approximate)
    var genesisTime = new DateTimeOffset(2023, 2, 15, 14, 0, 0, TimeSpan.Zero);
    var genesisRound = 7000000L;
    var roundPeriod = 3; // seconds
    
    var futureTime = DateTimeOffset.UtcNow.AddHours(hoursFromNow);
    var elapsed = futureTime - genesisTime;
    return genesisRound + (long)(elapsed.TotalSeconds / roundPeriod);
}
```

### Custom HttpClient Configuration

```csharp
services.AddHttpClient<DrandRandomSource>(client =>
{
    client.BaseAddress = new Uri("https://api.drand.sh/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

services.AddCryptoFairPickerDrand();
```

### Publishing Transcripts

Create verifiable transcripts for public draws:

```csharp
var round = RoundId.FromRound(9000000);
var participants = 100;
var winner = await selector.PickWinnerAsync(participants, round);

var transcript = $@"
Draw Transcript
===============
Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
Participants: {participants}
Drand Round: {round.Value}
Drand Chain: quicknet (52db9ba7...)
Verification URL: https://api.drand.sh/public/52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971/{round.Value}
Winner: #{winner}

Anyone can verify this draw by:
1. Fetching randomness from the URL above
2. Running CryptoFairPicker with round {round.Value} and {participants} participants
3. Confirming the winner is #{winner}
";

Console.WriteLine(transcript);
// Save transcript to file or publish
File.WriteAllText("draw-transcript.txt", transcript);
```

## 🔒 Security & Fairness

### Uniform Distribution

CryptoFairPicker uses **rejection sampling** to ensure uniform distribution without modulo bias:

```csharp
// ❌ WRONG: Modulo bias
var biased = randomValue % n;

// ✅ CORRECT: Rejection sampling
var maxValue = ulong.MaxValue - (ulong.MaxValue % (ulong)n);
if (randomValue < maxValue)
    return (int)(randomValue % (ulong)n);
// Otherwise, try again with next value
```

### Randomness Derivation

The library applies SHA-256 to drand's randomness for additional entropy:

```csharp
var randomness = FetchFromDrand(round);
var derivedBlock = SHA256.HashData(randomness); // 32-byte block
var result = MapToRange(derivedBlock, n);       // [0, n) using rejection sampling
```

### Error Handling

Robust error handling for network issues:

- **Timeouts**: Configurable timeout for HTTP requests (default 10s)
- **Retries**: Automatic retry with exponential backoff (default 3 attempts)
- **Clear errors**: Descriptive error messages when rounds don't exist or network fails

## 📊 Testing

Run the comprehensive test suite:

```bash
dotnet test
```

The test suite includes:
- **Determinism tests**: Same round + n produces same winner
- **Uniformity tests**: Distribution checks over many draws
- **Error handling tests**: Invalid inputs, network failures
- **Integration tests**: (optional) Live drand API calls

## 📖 Examples

See [samples/CryptoFairPicker.Sample](samples/CryptoFairPicker.Sample) for a complete working example.

Run it:

```bash
cd samples/CryptoFairPicker.Sample
dotnet run
```

## 🔄 Backward Compatibility

The existing `IPickerStrategy` and `IFairPicker` interfaces remain available:

```csharp
// Old API still works
services.AddCryptoFairPicker();  // CSPRNG
services.AddDrandBeaconPicker(); // Drand (old API)

var picker = provider.GetRequiredService<IFairPicker>();
var winner = picker.PickWinner(10); // 0-indexed [0, 9]
```

## 🗺️ Roadmap

- [ ] BLS signature verification for paranoid mode
- [ ] Support for additional drand chains
- [ ] Batch selection (pick multiple winners)
- [ ] Integration with Azure Key Vault for secrets
- [ ] GraphQL API example

## 📜 License

MIT License - see [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 🙏 Credits

- [Drand](https://drand.love/) - Distributed randomness beacon
- [League of Entropy](https://www.cloudflare.com/leagueofentropy/) - Drand network operators
- Inspired by real-world needs for transparent, verifiable randomness

## 📞 Support

- 📚 [Documentation](docs/)
- 🐛 [Issue Tracker](https://github.com/Tricksfor-Organization/CryptoFairPicker/issues)
- 💬 [Discussions](https://github.com/Tricksfor-Organization/CryptoFairPicker/discussions)

---

**Built with ❤️ for fair and transparent randomness**
