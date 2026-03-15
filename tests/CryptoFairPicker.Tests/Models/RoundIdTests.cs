using CryptoFairPicker.Models;
using NUnit.Framework;

namespace CryptoFairPicker.Tests.Models;

public class RoundIdTests
{
    // Quicknet genesis: 2023-08-23T15:29:27Z (Unix 1692803367), period 3s
    private static readonly DateTimeOffset QuicknetGenesis =
        DateTimeOffset.FromUnixTimeSeconds(1692803367);

    [Test]
    public void FromTime_AtGenesis_ReturnsRound1()
    {
        var round = RoundId.FromTime(QuicknetGenesis);
        Assert.That(round.GetRoundNumber(), Is.EqualTo(1));
    }

    [Test]
    public void FromTime_ThreeSecondsAfterGenesis_ReturnsRound2()
    {
        var round = RoundId.FromTime(QuicknetGenesis.AddSeconds(3));
        Assert.That(round.GetRoundNumber(), Is.EqualTo(2));
    }

    [Test]
    public void FromTime_WithinFirstPeriod_ReturnsRound1()
    {
        // 2 seconds after genesis is still in the first period
        var round = RoundId.FromTime(QuicknetGenesis.AddSeconds(2));
        Assert.That(round.GetRoundNumber(), Is.EqualTo(1));
    }

    [Test]
    public void FromTime_OneHourAfterGenesis_ReturnsExpectedRound()
    {
        // 3600 seconds / 3 = 1200 periods → round 1201
        var round = RoundId.FromTime(QuicknetGenesis.AddHours(1));
        Assert.That(round.GetRoundNumber(), Is.EqualTo(1201));
    }

    [Test]
    public void FromTime_BeforeGenesis_ThrowsArgumentOutOfRangeException()
    {
        var beforeGenesis = QuicknetGenesis.AddSeconds(-1);
        Assert.Throws<ArgumentOutOfRangeException>(() => RoundId.FromTime(beforeGenesis));
    }

    [Test]
    public void GetEstimatedTime_Round1_ReturnsGenesis()
    {
        var round = RoundId.FromRound(1);
        Assert.That(round.GetEstimatedTime(), Is.EqualTo(QuicknetGenesis));
    }

    [Test]
    public void GetEstimatedTime_Round2_ReturnsGenesisPlus3Seconds()
    {
        var round = RoundId.FromRound(2);
        Assert.That(round.GetEstimatedTime(), Is.EqualTo(QuicknetGenesis.AddSeconds(3)));
    }

    [Test]
    public void GetEstimatedTime_Round1201_ReturnsGenesisPlus1Hour()
    {
        var round = RoundId.FromRound(1201);
        Assert.That(round.GetEstimatedTime(), Is.EqualTo(QuicknetGenesis.AddHours(1)));
    }

    [Test]
    public void GetEstimatedTime_NonNumericValue_ThrowsInvalidOperationException()
    {
        var round = new RoundId("not-a-number");
        Assert.Throws<InvalidOperationException>(() => round.GetEstimatedTime());
    }

    [Test]
    public void GetEstimatedTime_ZeroRound_ThrowsInvalidOperationException()
    {
        var round = RoundId.FromRound(0);
        Assert.Throws<InvalidOperationException>(() => round.GetEstimatedTime());
    }

    [Test]
    public void GetEstimatedTime_NegativeRound_ThrowsInvalidOperationException()
    {
        var round = RoundId.FromRound(-5);
        Assert.Throws<InvalidOperationException>(() => round.GetEstimatedTime());
    }

    [Test]
    public void FromTime_RoundTrip_ReturnsOriginalTimeAlignedToRound()
    {
        // A time exactly on a round boundary should round-trip cleanly
        var time = QuicknetGenesis.AddSeconds(300); // exactly round 101
        var round = RoundId.FromTime(time);
        var estimatedTime = round.GetEstimatedTime();
        Assert.That(estimatedTime, Is.EqualTo(time));
    }

    [Test]
    public void FromTime_RoundTrip_MidPeriod_FloorsToRoundStart()
    {
        // A time mid-period floors to the round start
        var time = QuicknetGenesis.AddSeconds(301); // 1 second into round 102's period
        var round = RoundId.FromTime(time);
        Assert.That(round.GetRoundNumber(), Is.EqualTo(101));
        Assert.That(round.GetEstimatedTime(), Is.EqualTo(QuicknetGenesis.AddSeconds(300)));
    }
}
