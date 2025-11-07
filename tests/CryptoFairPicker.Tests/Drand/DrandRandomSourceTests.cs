using CryptoFairPicker.Drand;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Net;
using NUnit.Framework;

namespace CryptoFairPicker.Tests.Drand;

public class DrandRandomSourceTests
{
    [Test]
    public async Task NextIntAsync_ReturnsValueInRange()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789");
        var httpClient = new HttpClient(mockHandler);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);
        var round = RoundId.FromRound(1000);

        // Act
        var result = await source.NextIntAsync(10, round);

        // Assert
        Assert.That(result, Is.InRange(0, 9));
    }

    [Test]
    public async Task NextIntAsync_IsDeterministicForSameRound()
    {
        // Arrange - Same randomness should produce same result
        const string fixedRandomness = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var mockHandler1 = CreateMockHttpHandler(fixedRandomness);
        var mockHandler2 = CreateMockHttpHandler(fixedRandomness);
        
        var httpClient1 = new HttpClient(mockHandler1);
        var httpClient2 = new HttpClient(mockHandler2);
        
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source1 = new DrandRandomSource(httpClient1, options);
        var source2 = new DrandRandomSource(httpClient2, options);
        
        var round = RoundId.FromRound(1000);

        // Act
        var result1 = await source1.NextIntAsync(10, round);
        var result2 = await source2.NextIntAsync(10, round);

        // Assert
        Assert.That(result1, Is.EqualTo(result2));
    }

    [Test]
    public async Task NextIntAsync_DifferentRandomnessGivesDifferentResults()
    {
        // Arrange
        var options = Options.Create(new DrandOptions { RetryCount = 0 });

        // Assert - Different randomness should likely give different results (not guaranteed but highly probable)
        // We test this multiple times to be confident
        var differences = 0;
        for (int i = 0; i < 10; i++)
        {
            // Create distinct 64-character hex strings
            var hex1 = new string('a', 63) + i.ToString("x");
            var hex2 = new string('b', 63) + i.ToString("x");
            var mockH1 = CreateMockHttpHandler(hex1);
            var mockH2 = CreateMockHttpHandler(hex2);
            var hc1 = new HttpClient(mockH1);
            var hc2 = new HttpClient(mockH2);
            var s1 = new DrandRandomSource(hc1, options);
            var s2 = new DrandRandomSource(hc2, options);
            var r1 = await s1.NextIntAsync(100, RoundId.FromRound(i));
            var r2 = await s2.NextIntAsync(100, RoundId.FromRound(i));
            if (r1 != r2) differences++;
        }
        Assert.That(differences, Is.GreaterThan(5), "Different randomness should produce different results in most cases");
    }

    [Test]
    public void NextInt_ReturnsValueInRange()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210");
        var httpClient = new HttpClient(mockHandler);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);
        var round = RoundId.FromRound(1000);

        // Act
        var result = source.NextInt(10, round);

        // Assert
        Assert.That(result, Is.InRange(0, 9));
    }

    [Test]
    public void NextIntAsync_ThrowsForInvalidBound()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("test");
        var httpClient = new HttpClient(mockHandler);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);
        var round = RoundId.FromRound(1000);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await source.NextIntAsync(0, round));
        Assert.ThrowsAsync<ArgumentException>(async () => await source.NextIntAsync(-1, round));
    }

    [Test]
    public void NextIntAsync_ThrowsForNullRound()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("test");
        var httpClient = new HttpClient(mockHandler);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await source.NextIntAsync(10, null!));
    }

    [Test]
    public void NextIntAsync_ThrowsWhenRandomnessFieldMissing()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{""round"": 1000}")
        };

        var httpClient = new HttpClient(new TestHttpMessageHandler(response));
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);
        var round = RoundId.FromRound(1000);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await source.NextIntAsync(10, round));
        Assert.That(exception!.Message, Does.Contain("randomness"));
    }

    [Test]
    public async Task GetRoundInfoAsync_ReturnsRoundData()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("test123", 1234);
        var httpClient = new HttpClient(mockHandler);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);
        var round = RoundId.FromRound(1234);

        // Act
        var roundInfo = await source.GetRoundInfoAsync(round);

        // Assert
        Assert.That(roundInfo, Is.Not.Null);
        Assert.That(roundInfo, Does.Contain("1234"));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(10)]
    [TestCase(100)]
    public async Task NextIntAsync_ProducesUniformDistribution(int toExclusive)
    {
        // This is a basic sanity check, not a rigorous statistical test
        // We generate multiple values and ensure they span the range
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var counts = new int[toExclusive];
        var iterations = Math.Max(toExclusive * 5, 50);

        for (int i = 0; i < iterations; i++)
        {
            // Create distinct 64-character hex string for each iteration
            var hex = i.ToString("x").PadLeft(64, 'f');
            var mockHandler = CreateMockHttpHandler(hex);
            var httpClient = new HttpClient(mockHandler);
            var source = new DrandRandomSource(httpClient, options);
            var round = RoundId.FromRound(i);
            
            var result = await source.NextIntAsync(toExclusive, round);
            counts[result]++;
        }

        // Check that at least some buckets got at least one hit
        var nonZeroBuckets = counts.Count(c => c > 0);
        var expectedMinBuckets = Math.Max(toExclusive / 4, 1);
        Assert.That(nonZeroBuckets, Is.GreaterThanOrEqualTo(expectedMinBuckets), 
            $"Expected at least {expectedMinBuckets} non-zero buckets, got {nonZeroBuckets}");
    }

    private static HttpMessageHandler CreateMockHttpHandler(string randomness, long round = 1000)
    {
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent($@"{{
                ""round"": {round},
                ""randomness"": ""{randomness}"",
                ""signature"": ""test""
            }}")
        };
        return new TestHttpMessageHandler(response);
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public TestHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
