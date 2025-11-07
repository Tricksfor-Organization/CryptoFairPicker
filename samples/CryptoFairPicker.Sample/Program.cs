using CryptoFairPicker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("=== CryptoFairPicker Sample: Using Drand Public Randomness Beacon ===\n");

// Build configuration
var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Register CryptoFairPicker services with drand strategy
builder.Services.AddCryptoFairPicker(builder.Configuration);

var host = builder.Build();

// Get the winner selector service
var winnerSelector = host.Services.GetRequiredService<IWinnerSelector>();

// Example 1: Simple winner selection
Console.WriteLine("Example 1: Simple Winner Selection");
Console.WriteLine("------------------------------------------");
const int participantCount = 10;
var round = RoundId.FromRound(GetCurrentDrandRound());

Console.WriteLine($"Participants: {participantCount}");
Console.WriteLine($"Drand Round: {round.Value}");
Console.WriteLine($"Drand URL: https://api.drand.sh/public/52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971/{round.Value}");
Console.WriteLine();

try
{
    var winner = await winnerSelector.PickWinnerAsync(participantCount, round);
    Console.WriteLine($"🎉 Winner: Participant #{winner}");
    Console.WriteLine();
    
    Console.WriteLine("Verification Transcript:");
    Console.WriteLine($"- Used drand quicknet chain (3-second rounds)");
    Console.WriteLine($"- Round: {round.Value}");
    Console.WriteLine($"- Participants: {participantCount}");
    Console.WriteLine($"- Winner: #{winner}");
    Console.WriteLine($"- Verify randomness at: https://api.drand.sh/public/52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971/{round.Value}");
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine("Make sure you have internet access and the drand beacon is reachable.");
    Console.WriteLine();
}

// Example 2: Pre-announced draw with specific round
Console.WriteLine("Example 2: Pre-announced Draw");
Console.WriteLine("------------------------------------------");
const int futureRound = 100; // In production, calculate future round based on time
var futureRoundId = RoundId.FromRound(GetCurrentDrandRound() + futureRound);

Console.WriteLine($"📢 Announcement: The draw will use drand round {futureRoundId.Value}");
Console.WriteLine($"   This round will be available approximately {futureRound * 3} seconds from now.");
Console.WriteLine($"   URL: https://api.drand.sh/public/52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971/{futureRoundId.Value}");
Console.WriteLine();

Console.WriteLine("Anyone can verify the draw after the round is published:");
Console.WriteLine($"  1. Wait for round {futureRoundId.Value} to be published at the URL above");
Console.WriteLine($"  2. Use the 'randomness' field from the JSON response");
Console.WriteLine($"  3. Apply SHA-256 to derive the selection");
Console.WriteLine($"  4. Use rejection sampling to map uniformly to [1, {participantCount}]");
Console.WriteLine();

// Example 3: Demonstrating determinism
Console.WriteLine("Example 3: Demonstrating Determinism");
Console.WriteLine("------------------------------------------");
var fixedRound = RoundId.FromRound(9000000); // An old round that definitely exists
Console.WriteLine($"Using fixed round: {fixedRound.Value}");
Console.WriteLine($"Calling PickWinner 3 times with the same round...");

try
{
    var result1 = await winnerSelector.PickWinnerAsync(100, fixedRound);
    var result2 = await winnerSelector.PickWinnerAsync(100, fixedRound);
    var result3 = await winnerSelector.PickWinnerAsync(100, fixedRound);
    
    Console.WriteLine($"Result 1: #{result1}");
    Console.WriteLine($"Result 2: #{result2}");
    Console.WriteLine($"Result 3: #{result3}");
    Console.WriteLine();
    
    if (result1 == result2 && result2 == result3)
    {
        Console.WriteLine("✓ Same round produces same winner (deterministic)");
    }
    else
    {
        Console.WriteLine("✗ Unexpected: different results for same round");
    }
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine("This may happen if the specified round doesn't exist or is too old.");
    Console.WriteLine();
}

Console.WriteLine("Sample completed successfully!");
Console.WriteLine("\nTo verify results manually, use curl:");
Console.WriteLine($"  curl https://api.drand.sh/public/52db9ba70e0cc0f6eaf7803dd07447a1f5477735fd3f661792ba94600c84e971/latest");

// Helper to calculate current drand round (approximate)
static long GetCurrentDrandRound()
{
    // Drand quicknet genesis time: 2023-02-15 14:00:00 UTC (approximately round 7000000)
    // Round period: 3 seconds
    var genesisTime = new DateTimeOffset(2023, 2, 15, 14, 0, 0, TimeSpan.Zero);
    var genesisRound = 7000000L;
    var roundPeriod = 3; // seconds
    
    var now = DateTimeOffset.UtcNow;
    var elapsed = now - genesisTime;
    var roundsSinceGenesis = (long)(elapsed.TotalSeconds / roundPeriod);
    
    return genesisRound + roundsSinceGenesis;
}
