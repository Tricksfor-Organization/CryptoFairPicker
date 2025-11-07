# CryptoFairPicker

CryptoFairPicker is a C# class library for fair, unbiased, and cryptographically secure winner selection. Designed for games, lotteries, and any scenario where fairness matters. Built with Dependency Injection support for easy integration into ASP.NET Core or other DI-enabled frameworks.

## Features

- **Interface-based Design**: Extensible architecture using `IFairPicker` and `IPickerStrategy`
- **Three Built-in Strategies**:
  1. **CSPRNG Strategy**: Uses `RandomNumberGenerator.GetInt32` for cryptographically secure randomness
  2. **Commit-Reveal Strategy**: Enables auditable and transparent draws with cryptographic verification
  3. **Drand Beacon Strategy**: Leverages external randomness beacons (drand) for verifiable public randomness
- **Uniform Distribution**: All strategies avoid modulo bias using proper rejection sampling
- **Dependency Injection Ready**: First-class support for Microsoft.Extensions.DependencyInjection
- **Comprehensive Tests**: Extensive unit tests validate fairness and correctness
- **Targets .NET 9.0**: Uses the latest .NET framework

## Installation

Add the package to your project:

```bash
dotnet add package CryptoFairPicker
```

Or add it directly to your `.csproj` file:

```xml
<PackageReference Include="CryptoFairPicker" Version="1.0.0" />
```

## Quick Start

### Basic Usage

```csharp
using CryptoFairPicker;
using CryptoFairPicker.Strategies;

// Create a picker with CSPRNG strategy
var strategy = new CsprngStrategy();
var picker = new FairPicker(strategy);

// Pick a winner from 10 options (returns 0-9)
int winner = picker.PickWinner(10);
Console.WriteLine($"Winner: {winner}");
```

### Using Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using CryptoFairPicker;

var services = new ServiceCollection();

// Register with default CSPRNG strategy
services.AddCryptoFairPicker();

// Or register with a specific strategy
services.AddCommitRevealPicker();
// services.AddDrandBeaconPicker();

var provider = services.BuildServiceProvider();
var picker = provider.GetRequiredService<IFairPicker>();

int winner = picker.PickWinner(100);
```

## Strategies

### 1. CSPRNG Strategy

The default strategy using `RandomNumberGenerator.GetInt32` for cryptographically secure randomness.

```csharp
var strategy = new CsprngStrategy();
var picker = new FairPicker(strategy);
int winner = picker.PickWinner(10);
```

**Benefits:**
- Fast and efficient
- Cryptographically secure
- No external dependencies

### 2. Commit-Reveal Strategy

Enables transparent and auditable draws. The commitment is made before the draw, and can be verified afterward.

```csharp
var strategy = new CommitRevealStrategy();

// Commit (can share this hash publicly before the draw)
strategy.Commit();
Console.WriteLine($"Commitment: {strategy.CommitmentHash}");

// Reveal and pick winner
int winner = strategy.Pick(10);
Console.WriteLine($"Winner: {winner}");
Console.WriteLine($"Secret: {strategy.Secret}");

// Anyone can verify the draw was fair
bool isValid = strategy.Verify(strategy.Secret!, 10, winner);
Console.WriteLine($"Valid: {isValid}");
```

**Benefits:**
- Auditable and transparent
- Prevents manipulation after commitment
- Cryptographic proof of fairness

### 3. Drand Beacon Strategy

Uses public randomness from the drand network, a distributed randomness beacon.

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddDrandBeaconPicker(); // Uses default drand quicknet
// Or use a custom beacon URL:
// services.AddDrandBeaconPicker("https://custom.beacon.url/latest");

var provider = services.BuildServiceProvider();
var picker = provider.GetRequiredService<IFairPicker>();

int winner = await picker.PickWinnerAsync(10);
```

**Benefits:**
- Publicly verifiable randomness
- External source (not controlled by you)
- Ideal for high-stakes draws requiring public trust

## API Reference

### IFairPicker

```csharp
public interface IFairPicker
{
    int PickWinner(int optionCount);
    Task<int> PickWinnerAsync(int optionCount, CancellationToken cancellationToken = default);
}
```

### IPickerStrategy

```csharp
public interface IPickerStrategy
{
    int Pick(int optionCount);
    Task<int> PickAsync(int optionCount, CancellationToken cancellationToken = default);
}
```

### Extension Methods

```csharp
// Add CSPRNG picker (default)
services.AddCryptoFairPicker();

// Add with custom strategy
services.AddCryptoFairPicker<MyCustomStrategy>();

// Add Commit-Reveal picker
services.AddCommitRevealPicker();

// Add Drand Beacon picker
services.AddDrandBeaconPicker(beaconUrl: null); // null uses default
```

## Examples

See the [examples](examples/CryptoFairPicker.Examples) directory for complete working examples including:
- Direct usage with each strategy
- Dependency injection integration
- Fairness demonstration
- Async operations
- Commit-Reveal verification

Run the examples:

```bash
cd examples/CryptoFairPicker.Examples
dotnet run
```

## Testing

The library includes comprehensive unit tests covering:
- Uniform distribution verification
- Correctness of each strategy
- Dependency injection registration
- Edge cases and error handling

Run tests:

```bash
dotnet test
```

## Fairness & Security

### Uniform Distribution

All strategies ensure uniform distribution without modulo bias:
- **CSPRNG**: Uses `RandomNumberGenerator.GetInt32` which internally uses rejection sampling
- **Commit-Reveal**: Derives values using HMAC-SHA256 from cryptographic randomness
- **Drand Beacon**: Hashes external randomness with SHA-256 for uniform distribution

### Cryptographic Security

- Uses `System.Security.Cryptography.RandomNumberGenerator` for secure randomness
- Commit-Reveal uses SHA-256 for commitments and HMAC-SHA256 for derivation
- No use of weak PRNGs or predictable seeds

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

Created for fair and transparent selection in games, lotteries, and any scenario where cryptographic fairness matters.
