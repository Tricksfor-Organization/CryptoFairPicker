# Verification Guide

This guide demonstrates how to verify CryptoFairPicker's drand-based winner selection independently.

## Understanding Drand

[Drand](https://drand.love/) is a distributed randomness beacon that provides publicly verifiable, unbiased, and unpredictable random numbers. The quicknet chain produces a new random value every 3 seconds.

## Step 1: Fetch Randomness from Drand

Use `curl` to fetch randomness from a specific round:

```bash
# Get the latest round
curl https://api.drand.sh/public/52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971/latest

# Get a specific round (e.g., round 9000000)
curl https://api.drand.sh/public/52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971/9000000
```

The response will look like:
```json
{
  "round": 9000000,
  "randomness": "a1b2c3d4e5f6...",
  "signature": "...",
  "previous_signature": "..."
}
```

## Step 2: Verify with CryptoFairPicker

Create a simple C# program to verify the selection:

```csharp
using CryptoFairPicker;
using CryptoFairPicker.Drand;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// Setup DI
var services = new ServiceCollection();
services.AddCryptoFairPickerDrand();
var provider = services.BuildServiceProvider();

// Get the winner selector
var selector = provider.GetRequiredService<IWinnerSelector>();

// Pick winner for round 9000000 with 100 participants
var round = RoundId.FromRound(9000000);
var winner = await selector.PickWinnerAsync(100, round);

Console.WriteLine($"Winner: {winner}");
```

## Step 3: Manual Verification

You can manually verify the selection process:

### 3.1 Extract Randomness

From the drand JSON response, extract the `randomness` field (hex-encoded).

Example: `"randomness": "a1b2c3d4..."`

### 3.2 Derive Random Block

Apply SHA-256 to the randomness bytes:

```bash
echo -n "a1b2c3d4..." | xxd -r -p | sha256sum
```

Or in C#:
```csharp
using System.Security.Cryptography;

var randomnessHex = "a1b2c3d4...";
var randomness = Convert.FromHexString(randomnessHex);
var derivedBlock = SHA256.HashData(randomness);
```

### 3.3 Map to Range Using Rejection Sampling

The library uses rejection sampling to avoid modulo bias:

```csharp
using System.Security.Cryptography;

byte[] MapToRangeWithRejectionSampling(byte[] randomBlock, int n)
{
    using var hmac = new HMACSHA256(randomBlock);
    int counter = 0;
    
    while (true)
    {
        var input = BitConverter.GetBytes(counter);
        var hash = hmac.ComputeHash(input);
        var value = BitConverter.ToUInt64(hash, 0);
        
        var maxValue = ulong.MaxValue - (ulong.MaxValue % (ulong)n);
        
        if (value < maxValue)
        {
            return (int)(value % (ulong)n) + 1; // +1 for 1-indexed
        }
        
        counter++;
    }
}
```

## Step 4: Verify Determinism

The same round should always produce the same winner:

```bash
# Run your program multiple times with the same round
dotnet run -- --round 9000000 --participants 100
dotnet run -- --round 9000000 --participants 100
dotnet run -- --round 9000000 --participants 100

# All three runs should output the same winner
```

## Step 5: Verify with curl and jq

You can create a complete verification pipeline:

```bash
#!/bin/bash

ROUND=9000000
PARTICIPANTS=100

# Fetch randomness
RANDOMNESS=$(curl -s "https://api.drand.sh/public/52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971/$ROUND" | jq -r .randomness)

echo "Round: $ROUND"
echo "Randomness: $RANDOMNESS"

# Use your verification tool or CryptoFairPicker to compute winner
dotnet run --project /path/to/CryptoFairPicker.Sample -- --verify-round $ROUND --participants $PARTICIPANTS
```

## Understanding Round Scheduling

Drand quicknet produces rounds every 3 seconds. To calculate a future round:

```csharp
// Genesis parameters (approximate)
var genesisTime = new DateTimeOffset(2023, 2, 15, 14, 0, 0, TimeSpan.Zero);
var genesisRound = 7000000L;
var roundPeriod = 3; // seconds

// Calculate current round
var now = DateTimeOffset.UtcNow;
var elapsed = now - genesisTime;
var currentRound = genesisRound + (long)(elapsed.TotalSeconds / roundPeriod);

// Calculate future round (e.g., 1 hour from now)
var futureTime = now.AddHours(1);
var futureElapsed = futureTime - genesisTime;
var futureRound = genesisRound + (long)(futureElapsed.TotalSeconds / roundPeriod);

Console.WriteLine($"Current round: {currentRound}");
Console.WriteLine($"Round in 1 hour: {futureRound}");
```

## Pre-announced Draws

For maximum transparency, announce the round number before it's published:

1. Calculate a future round (e.g., 24 hours from now)
2. Publish the round number and participant list
3. Wait for the round to be published by drand
4. Run the selection using that round
5. Anyone can verify using the same round and participant count

## Security Considerations

- **Verification**: Always verify that the randomness comes from the official drand API
- **Round Timing**: Ensure the round hasn't been published when making pre-announcements
- **Signature Verification**: For critical applications, verify the BLS signature on the drand beacon
- **Determinism**: The same round + participant count always produces the same winner

## Resources

- [Drand Documentation](https://drand.love/)
- [Drand API Documentation](https://drand.love/developer/http-api/)
- [League of Entropy](https://www.cloudflare.com/leagueofentropy/)
- [Drand Verification Tools](https://github.com/drand/drand)

## Troubleshooting

### "Round not found" error

The round might be too old or not yet published. Check:
```bash
curl https://api.drand.sh/public/52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971/info
```

This shows the current round and chain information.

### Network timeout

Drand relays may occasionally be unavailable. The library includes retry logic, but for critical applications, consider running your own drand relay.

### Different results

If you get different results:
1. Verify you're using the exact same round number
2. Verify you're using the exact same participant count
3. Check that you're using the same drand chain
4. Ensure you're using the same algorithm (SHA-256 + HMAC-SHA256 + rejection sampling)
