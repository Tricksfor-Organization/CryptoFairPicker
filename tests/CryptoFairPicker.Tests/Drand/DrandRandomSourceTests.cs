using CryptoFairPicker.Drand;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace CryptoFairPicker.Tests.Drand;

public class DrandRandomSourceTests
{
    [Fact]
    public async Task NextIntAsync_ReturnsValueInRange()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789");
        var httpClient = new HttpClient(mockHandler.Object);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);
        var round = RoundId.FromRound(1000);

        // Act
        var result = await source.NextIntAsync(10, round);

        // Assert
        Assert.InRange(result, 0, 9);
    }

    [Fact]
    public async Task NextIntAsync_IsDeterministicForSameRound()
    {
        // Arrange - Same randomness should produce same result
        const string fixedRandomness = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var mockHandler1 = CreateMockHttpHandler(fixedRandomness);
        var mockHandler2 = CreateMockHttpHandler(fixedRandomness);
        
        var httpClient1 = new HttpClient(mockHandler1.Object);
        var httpClient2 = new HttpClient(mockHandler2.Object);
        
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source1 = new DrandRandomSource(httpClient1, options);
        var source2 = new DrandRandomSource(httpClient2, options);
        
        var round = RoundId.FromRound(1000);

        // Act
        var result1 = await source1.NextIntAsync(10, round);
        var result2 = await source2.NextIntAsync(10, round);

        // Assert
        Assert.Equal(result1, result2);
    }

    [Fact]
    public async Task NextIntAsync_DifferentRandomnessGivesDifferentResults()
    {
        // Arrange
        var mockHandler1 = CreateMockHttpHandler("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        var mockHandler2 = CreateMockHttpHandler("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
        
        var httpClient1 = new HttpClient(mockHandler1.Object);
        var httpClient2 = new HttpClient(mockHandler2.Object);
        
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source1 = new DrandRandomSource(httpClient1, options);
        var source2 = new DrandRandomSource(httpClient2, options);
        
        var round = RoundId.FromRound(1000);

        // Act
        var result1 = await source1.NextIntAsync(100, round);
        var result2 = await source2.NextIntAsync(100, round);

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
            var hc1 = new HttpClient(mockH1.Object);
            var hc2 = new HttpClient(mockH2.Object);
            var s1 = new DrandRandomSource(hc1, options);
            var s2 = new DrandRandomSource(hc2, options);
            var r1 = await s1.NextIntAsync(100, RoundId.FromRound(i));
            var r2 = await s2.NextIntAsync(100, RoundId.FromRound(i));
            if (r1 != r2) differences++;
        }
        Assert.True(differences > 5, "Different randomness should produce different results in most cases");
    }

    [Fact]
    public void NextInt_ReturnsValueInRange()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210");
        var httpClient = new HttpClient(mockHandler.Object);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);
        var round = RoundId.FromRound(1000);

        // Act
        var result = source.NextInt(10, round);

        // Assert
        Assert.InRange(result, 0, 9);
    }

    [Fact]
    public async Task NextIntAsync_ThrowsForInvalidBound()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("test");
        var httpClient = new HttpClient(mockHandler.Object);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);
        var round = RoundId.FromRound(1000);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => source.NextIntAsync(0, round));
        await Assert.ThrowsAsync<ArgumentException>(() => source.NextIntAsync(-1, round));
    }

    [Fact]
    public async Task NextIntAsync_ThrowsForNullRound()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("test");
        var httpClient = new HttpClient(mockHandler.Object);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => source.NextIntAsync(10, null!));
    }

    [Fact]
    public async Task NextIntAsync_ThrowsWhenRandomnessFieldMissing()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{""round"": 1000}")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);
        var round = RoundId.FromRound(1000);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => source.NextIntAsync(10, round));
        Assert.Contains("randomness", exception.Message);
    }

    [Fact]
    public async Task GetRoundInfoAsync_ReturnsRoundData()
    {
        // Arrange
        var mockHandler = CreateMockHttpHandler("test123", 1234);
        var httpClient = new HttpClient(mockHandler.Object);
        var options = Options.Create(new DrandOptions { RetryCount = 0 });
        var source = new DrandRandomSource(httpClient, options);
        var round = RoundId.FromRound(1234);

        // Act
        var roundInfo = await source.GetRoundInfoAsync(round);

        // Assert
        Assert.NotNull(roundInfo);
        Assert.Contains("1234", roundInfo);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(100)]
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
            var httpClient = new HttpClient(mockHandler.Object);
            var source = new DrandRandomSource(httpClient, options);
            var round = RoundId.FromRound(i);
            
            var result = await source.NextIntAsync(toExclusive, round);
            counts[result]++;
        }

        // Check that at least some buckets got at least one hit
        var nonZeroBuckets = counts.Count(c => c > 0);
        var expectedMinBuckets = Math.Max(toExclusive / 4, 1);
        Assert.True(nonZeroBuckets >= expectedMinBuckets, 
            $"Expected at least {expectedMinBuckets} non-zero buckets, got {nonZeroBuckets}");
    }

    private static Mock<HttpMessageHandler> CreateMockHttpHandler(string randomness, long round = 1000)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent($@"{{
                    ""round"": {round},
                    ""randomness"": ""{randomness}"",
                    ""signature"": ""test""
                }}")
            });
        return mockHandler;
    }
}
