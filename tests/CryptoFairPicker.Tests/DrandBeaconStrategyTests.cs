using CryptoFairPicker.Strategies;
using System.Net;
using NUnit.Framework;

namespace CryptoFairPicker.Tests;

public class DrandBeaconStrategyTests
{
    [Test]
    public async Task PickAsync_ReturnsValueInRange()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{
                ""round"": 1000,
                ""randomness"": ""abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789"",
                ""signature"": ""test""
            }")
        };

        var httpClient = new HttpClient(new TestHttpMessageHandler(response));
        var strategy = new DrandBeaconStrategy(httpClient);
        const int optionCount = 10;

        // Act
        var result = await strategy.PickAsync(optionCount);

        // Assert
        Assert.That(result, Is.InRange(0, optionCount - 1));
    }

    [Test]
    public void PickAsync_ThrowsForInvalidOptionCount()
    {
        // Arrange
        var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{}") };
        var httpClient = new HttpClient(new TestHttpMessageHandler(response));
        var strategy = new DrandBeaconStrategy(httpClient);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await strategy.PickAsync(0));
        Assert.ThrowsAsync<ArgumentException>(async () => await strategy.PickAsync(-1));
    }

    [Test]
    public async Task FetchRandomnessAsync_ParsesResponse()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{
                ""round"": 1000,
                ""randomness"": ""0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"",
                ""signature"": ""test""
            }")
        };

        var httpClient = new HttpClient(new TestHttpMessageHandler(response));
        var strategy = new DrandBeaconStrategy(httpClient);

        // Act
        var randomness = await strategy.FetchRandomnessAsync();

        // Assert
        Assert.That(randomness, Is.Not.Null);
        Assert.That(randomness, Is.Not.Empty);
    }

    [Test]
    public void Pick_ReturnsValueInRange()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{
                ""round"": 1000,
                ""randomness"": ""fedcba9876543210fedcba9876543210fedcba9876543210fedcba9876543210"",
                ""signature"": ""test""
            }")
        };

        var httpClient = new HttpClient(new TestHttpMessageHandler(response));
        var strategy = new DrandBeaconStrategy(httpClient);
        const int optionCount = 10;

        // Act
        var result = strategy.Pick(optionCount);

        // Assert
        Assert.That(result, Is.InRange(0, optionCount - 1));
    }

    [Test]
    public void Constructor_ThrowsForNullHttpClient()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DrandBeaconStrategy(null!));
    }

    [Test]
    public async Task GetRoundInfoAsync_ReturnsRoundData()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{
                ""round"": 1234,
                ""randomness"": ""test"",
                ""signature"": ""test""
            }")
        };

        var httpClient = new HttpClient(new TestHttpMessageHandler(response));
        var strategy = new DrandBeaconStrategy(httpClient);

        // Act
        var roundInfo = await strategy.GetRoundInfoAsync(1234);

        // Assert
        Assert.That(roundInfo, Is.Not.Null);
        Assert.That(roundInfo, Does.Contain("1234"));
    }

    [Test]
    public async Task PickAsync_IsDeterministicForSameRandomness()
    {
        // Arrange
        const string fixedRandomness = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var response1 = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent($@"{{
                ""round"": 1000,
                ""randomness"": ""{fixedRandomness}"",
                ""signature"": ""test""
            }}")
        };

        var response2 = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent($@"{{
                ""round"": 1000,
                ""randomness"": ""{fixedRandomness}"",
                ""signature"": ""test""
            }}")
        };

        var httpClient1 = new HttpClient(new TestHttpMessageHandler(response1));
        var httpClient2 = new HttpClient(new TestHttpMessageHandler(response2));
        var strategy1 = new DrandBeaconStrategy(httpClient1);
        var strategy2 = new DrandBeaconStrategy(httpClient2);
        const int optionCount = 10;

        // Act
        var result1 = await strategy1.PickAsync(optionCount);
        var result2 = await strategy2.PickAsync(optionCount);

        // Assert - Same randomness should produce same result
        Assert.That(result1, Is.EqualTo(result2));
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
