using CryptoFairPicker.Strategies;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace CryptoFairPicker.Tests;

public class DrandBeaconStrategyTests
{
    [Fact]
    public async Task PickAsync_ReturnsValueInRange()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""round"": 1000,
                    ""randomness"": ""abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789"",
                    ""signature"": ""test""
                }")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var strategy = new DrandBeaconStrategy(httpClient);
        const int optionCount = 10;

        // Act
        var result = await strategy.PickAsync(optionCount);

        // Assert
        Assert.InRange(result, 0, optionCount - 1);
    }

    [Fact]
    public async Task PickAsync_ThrowsForInvalidOptionCount()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var strategy = new DrandBeaconStrategy(httpClient);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => strategy.PickAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => strategy.PickAsync(-1));
    }

    [Fact]
    public async Task FetchRandomnessAsync_ParsesResponse()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""round"": 1000,
                    ""randomness"": ""0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"",
                    ""signature"": ""test""
                }")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var strategy = new DrandBeaconStrategy(httpClient);

        // Act
        var randomness = await strategy.FetchRandomnessAsync();

        // Assert
        Assert.NotNull(randomness);
        Assert.NotEmpty(randomness);
    }

    [Fact]
    public void Pick_ReturnsValueInRange()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""round"": 1000,
                    ""randomness"": ""fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210"",
                    ""signature"": ""test""
                }")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var strategy = new DrandBeaconStrategy(httpClient);
        const int optionCount = 10;

        // Act
        var result = strategy.Pick(optionCount);

        // Assert
        Assert.InRange(result, 0, optionCount - 1);
    }

    [Fact]
    public void Constructor_ThrowsForNullHttpClient()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DrandBeaconStrategy(null!));
    }

    [Fact]
    public async Task GetRoundInfoAsync_ReturnsRoundData()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"{
                    ""round"": 1234,
                    ""randomness"": ""test"",
                    ""signature"": ""test""
                }")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var strategy = new DrandBeaconStrategy(httpClient);

        // Act
        var roundInfo = await strategy.GetRoundInfoAsync(1234);

        // Assert
        Assert.NotNull(roundInfo);
        Assert.Contains("1234", roundInfo);
    }

    [Fact]
    public async Task PickAsync_IsDeterministicForSameRandomness()
    {
        // Arrange
        const string fixedRandomness = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var mockHttpMessageHandler1 = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler1
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
                    ""round"": 1000,
                    ""randomness"": ""{fixedRandomness}"",
                    ""signature"": ""test""
                }}")
            });

        var mockHttpMessageHandler2 = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler2
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
                    ""round"": 1000,
                    ""randomness"": ""{fixedRandomness}"",
                    ""signature"": ""test""
                }}")
            });

        var httpClient1 = new HttpClient(mockHttpMessageHandler1.Object);
        var httpClient2 = new HttpClient(mockHttpMessageHandler2.Object);
        var strategy1 = new DrandBeaconStrategy(httpClient1);
        var strategy2 = new DrandBeaconStrategy(httpClient2);
        const int optionCount = 10;

        // Act
        var result1 = await strategy1.PickAsync(optionCount);
        var result2 = await strategy2.PickAsync(optionCount);

        // Assert - Same randomness should produce same result
        Assert.Equal(result1, result2);
    }
}
