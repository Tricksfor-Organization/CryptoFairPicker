using CryptoFairPicker;
using CryptoFairPicker.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("=== CryptoFairPicker Examples ===\n");

// Example 1: Direct usage with CSPRNG Strategy
Console.WriteLine("Example 1: Using CSPRNG Strategy directly");
Console.WriteLine("------------------------------------------");
var csprngStrategy = new CsprngStrategy();
var csprngPicker = new FairPicker(csprngStrategy);
var winner1 = csprngPicker.PickWinner(10);
Console.WriteLine($"Winner selected from 10 options: {winner1}");
Console.WriteLine();

// Example 2: Using Dependency Injection with CSPRNG
Console.WriteLine("Example 2: Using DI with CSPRNG Strategy");
Console.WriteLine("------------------------------------------");
var services = new ServiceCollection();
services.AddCryptoFairPicker(); // Default is CSPRNG
var provider = services.BuildServiceProvider();
var picker = provider.GetRequiredService<IFairPicker>();
var winner2 = picker.PickWinner(100);
Console.WriteLine($"Winner selected from 100 options: {winner2}");
Console.WriteLine();

// Example 3: Commit-Reveal Strategy for Auditable Draws
Console.WriteLine("Example 3: Using Commit-Reveal Strategy");
Console.WriteLine("------------------------------------------");
var commitRevealStrategy = new CommitRevealStrategy();
commitRevealStrategy.Commit();
Console.WriteLine($"Commitment Hash: {commitRevealStrategy.CommitmentHash}");
Console.WriteLine("(This can be shared before the draw)");
Console.WriteLine();

const int optionCount = 5;
var winner3 = commitRevealStrategy.Pick(optionCount);
Console.WriteLine($"Winner selected from {optionCount} options: {winner3}");
Console.WriteLine($"Secret: {commitRevealStrategy.Secret}");
Console.WriteLine();

// Verify the draw
var isValid = commitRevealStrategy.Verify(
    commitRevealStrategy.Secret!,
    optionCount,
    winner3);
Console.WriteLine($"Verification result: {(isValid ? "VALID" : "INVALID")}");
Console.WriteLine();

// Example 4: Multiple picks to demonstrate fairness
Console.WriteLine("Example 4: Demonstrating Fairness (1000 picks)");
Console.WriteLine("------------------------------------------");
var fairnessStrategy = new CsprngStrategy();
var fairnessPicker = new FairPicker(fairnessStrategy);
var counts = new int[5];
for (int i = 0; i < 1000; i++)
{
    var result = fairnessPicker.PickWinner(5);
    counts[result]++;
}

Console.WriteLine("Distribution of picks:");
for (int i = 0; i < counts.Length; i++)
{
    Console.WriteLine($"  Option {i}: {counts[i]} times ({counts[i] / 10.0}%)");
}
Console.WriteLine();

// Example 5: Async usage
Console.WriteLine("Example 5: Async Winner Selection");
Console.WriteLine("------------------------------------------");
var asyncPicker = new FairPicker(new CsprngStrategy());
var asyncWinner = await asyncPicker.PickWinnerAsync(20);
Console.WriteLine($"Async winner selected from 20 options: {asyncWinner}");
Console.WriteLine();

// Example 6: Using Commit-Reveal with DI
Console.WriteLine("Example 6: Commit-Reveal with DI");
Console.WriteLine("------------------------------------------");
var crServices = new ServiceCollection();
crServices.AddCommitRevealPicker();
var crProvider = crServices.BuildServiceProvider();
var crPicker = crProvider.GetRequiredService<IFairPicker>();
var crStrategy = crProvider.GetRequiredService<CommitRevealStrategy>();

// Show commitment before pick
Console.WriteLine($"Commitment (before reveal): {crStrategy.CommitmentHash ?? "Not committed yet"}");
var winner6 = crPicker.PickWinner(7);
Console.WriteLine($"Winner: {winner6}");
Console.WriteLine($"Secret (after reveal): {crStrategy.Secret}");
Console.WriteLine();

Console.WriteLine("Examples completed successfully!");
