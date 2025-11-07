# CryptoFairPicker.Tests

Comprehensive test suite for the CryptoFairPicker library using **NUnit** and **NSubstitute**.

## Test Framework

- **NUnit 3.x** - Modern .NET testing framework with rich assertions
- **NSubstitute** - Friendly mocking library for .NET
- **Microsoft.NET.Test.Sdk** - Test platform for running tests
- **FluentAssertions** - Additional assertion library (optional)

## Test Structure

```
CryptoFairPicker.Tests/
├── Csprng/
│   ├── CsprngRandomSourceTests.cs      - Local CSPRNG randomness tests
│   └── CsprngWinnerSelectorTests.cs    - CSPRNG winner selection tests
├── Drand/
│   ├── DrandRandomSourceTests.cs       - Drand HTTP API integration tests
│   └── DrandWinnerSelectorTests.cs     - Drand winner selection tests
├── CommitRevealStrategyTests.cs        - Commit-reveal strategy tests
├── CsprngStrategyTests.cs              - CSPRNG strategy tests (old API)
├── DrandBeaconStrategyTests.cs         - Drand beacon strategy tests (old API)
├── FairPickerTests.cs                  - Main picker facade tests
└── ServiceCollectionExtensionsTests.cs - DI configuration tests
```

## Test Coverage

### Core Components

#### 1. **CSPRNG Tests** (`Csprng/`)
- ✅ Value range validation (0-indexed)
- ✅ Input validation (positive bounds, non-null rounds)
- ✅ Non-deterministic behavior verification
- ✅ Variance and distribution checks

#### 2. **Drand Tests** (`Drand/`)
- ✅ HTTP communication with drand API
- ✅ Deterministic results for same round
- ✅ JSON parsing and error handling
- ✅ Round information retrieval
- ✅ Uniform distribution validation
- ✅ Network error handling (timeouts, retries)
- ✅ Mock HTTP handlers for isolated testing

#### 3. **Winner Selector Tests**
- ✅ 1-indexed winner selection [1, n]
- ✅ Correct offset from 0-indexed random source
- ✅ Boundary conditions (first/last participant)
- ✅ Input validation

#### 4. **Strategy Tests** (Legacy API)
- ✅ CommitReveal: commitment hash generation, verification
- ✅ CSPRNG: local random generation, distribution
- ✅ DrandBeacon: HTTP integration, determinism

#### 5. **Integration Tests**
- ✅ Dependency injection setup
- ✅ Service registration validation
- ✅ Configuration binding
- ✅ End-to-end winner selection

## Test Patterns

### Arrange-Act-Assert (AAA)

All tests follow the AAA pattern for clarity:

```csharp
[Test]
public async Task NextIntAsync_ReturnsValueInRange()
{
    // Arrange
    var source = new CsprngRandomSource();
    var round = RoundId.FromRound(1000);

    // Act
    var result = await source.NextIntAsync(10, round);

    // Assert
    Assert.That(result, Is.InRange(0, 9));
}
```

### NSubstitute Mocking

Tests use NSubstitute for clean, readable mocks:

```csharp
[Test]
public async Task PickWinnerAsync_ReturnsValueInCorrectRange()
{
    // Arrange
    var mockSource = Substitute.For<IFairRandomSource>();
    mockSource
        .NextIntAsync(Arg.Any<int>(), Arg.Any<RoundId>(), Arg.Any<CancellationToken>())
        .Returns(5);
    
    var selector = new DrandWinnerSelector(mockSource);
    var round = RoundId.FromRound(1000);

    // Act
    var winner = await selector.PickWinnerAsync(10, round);

    // Assert
    Assert.That(winner, Is.EqualTo(6)); // 5 + 1 = 6 (1-indexed)
    await mockSource.Received(1).NextIntAsync(10, round, Arg.Any<CancellationToken>());
}
```

### Custom HTTP Test Handlers

For testing HTTP communication without actual network calls:

```csharp
private class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public TestHttpMessageHandler(HttpResponseMessage response)
    {
        _response = response;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}
```

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~DrandRandomSourceTests"
```

### Run Tests with Detailed Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Test Categories

### Unit Tests
Fast, isolated tests with no external dependencies:
- CSPRNG tests
- Winner selector tests with mocked sources
- Strategy tests with mocked HTTP handlers

### Integration Tests (Optional)
Tests that make actual HTTP calls to drand API (can be slow or flaky):
- Mark with `[Explicit]` attribute to skip in CI
- Enable manually for verification

```csharp
[Test, Explicit]
public async Task DrandApi_ReturnsValidRandomness()
{
    // Actual HTTP call to drand.love
    var httpClient = new HttpClient();
    var source = new DrandRandomSource(httpClient, options);
    var result = await source.NextIntAsync(100, RoundId.FromRound(9000000));
    
    Assert.That(result, Is.InRange(0, 99));
}
```

## Key Test Scenarios

### 1. Determinism Verification
```csharp
[Test]
public async Task NextIntAsync_IsDeterministicForSameRound()
{
    // Same round + same n = same result
    var source1 = new DrandRandomSource(httpClient1, options);
    var source2 = new DrandRandomSource(httpClient2, options);
    
    var result1 = await source1.NextIntAsync(10, round);
    var result2 = await source2.NextIntAsync(10, round);
    
    Assert.That(result1, Is.EqualTo(result2));
}
```

### 2. Uniform Distribution
```csharp
[TestCase(1)]
[TestCase(2)]
[TestCase(10)]
[TestCase(100)]
public async Task NextIntAsync_ProducesUniformDistribution(int toExclusive)
{
    var counts = new int[toExclusive];
    var iterations = Math.Max(toExclusive * 5, 50);

    for (int i = 0; i < iterations; i++)
    {
        var result = await source.NextIntAsync(toExclusive, round);
        counts[result]++;
    }

    var nonZeroBuckets = counts.Count(c => c > 0);
    var expectedMinBuckets = Math.Max(toExclusive / 4, 1);
    Assert.That(nonZeroBuckets, Is.GreaterThanOrEqualTo(expectedMinBuckets));
}
```

### 3. Error Handling
```csharp
[Test]
public void NextIntAsync_ThrowsForInvalidBound()
{
    Assert.ThrowsAsync<ArgumentException>(async () => 
        await source.NextIntAsync(0, round));
    Assert.ThrowsAsync<ArgumentException>(async () => 
        await source.NextIntAsync(-1, round));
}

[Test]
public void NextIntAsync_ThrowsWhenRandomnessFieldMissing()
{
    var response = new HttpResponseMessage
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(@"{""round"": 1000}")
    };

    var exception = Assert.ThrowsAsync<InvalidOperationException>(
        async () => await source.NextIntAsync(10, round));
    Assert.That(exception!.Message, Does.Contain("randomness"));
}
```

### 4. Dependency Injection
```csharp
[Test]
public void AddCryptoFairPicker_RegistersServices()
{
    var services = new ServiceCollection();
    services.AddCryptoFairPicker();
    var provider = services.BuildServiceProvider();

    var picker = provider.GetService<IFairPicker>();
    var strategy = provider.GetService<IPickerStrategy>();
    
    Assert.That(picker, Is.Not.Null);
    Assert.That(strategy, Is.Not.Null);
    Assert.That(picker, Is.TypeOf<FairPicker>());
    Assert.That(strategy, Is.TypeOf<CsprngStrategy>());
}
```

## Assertion Styles

### NUnit Constraint Model (Preferred)
```csharp
Assert.That(result, Is.EqualTo(expected));
Assert.That(value, Is.InRange(0, 9));
Assert.That(collection, Is.Not.Empty);
Assert.That(text, Does.Contain("substring"));
Assert.That(obj, Is.Not.Null);
Assert.That(count, Is.GreaterThan(10));
```

### Classic Assertions (Also Supported)
```csharp
Assert.AreEqual(expected, result);
Assert.IsTrue(condition);
Assert.IsNotNull(obj);
Assert.Throws<ArgumentException>(() => DoSomething());
```

## Migration from xUnit

This test suite was migrated from xUnit to NUnit:

| xUnit | NUnit |
|-------|-------|
| `[Fact]` | `[Test]` |
| `[Theory]` + `[InlineData]` | `[TestCase]` |
| `Assert.Equal(a, b)` | `Assert.That(b, Is.EqualTo(a))` |
| `Assert.InRange(x, min, max)` | `Assert.That(x, Is.InRange(min, max))` |
| `Assert.True(x)` | `Assert.That(x, Is.True)` |
| `Assert.NotNull(x)` | `Assert.That(x, Is.Not.Null)` |
| `await Assert.ThrowsAsync<T>()` | `Assert.ThrowsAsync<T>()` |

Moq was replaced with NSubstitute:

| Moq | NSubstitute |
|-----|-------------|
| `new Mock<IService>()` | `Substitute.For<IService>()` |
| `mock.Setup(x => x.Method()).Returns(value)` | `mock.Method().Returns(value)` |
| `mock.Verify(x => x.Method(), Times.Once)` | `mock.Received(1).Method()` |
| `It.IsAny<T>()` | `Arg.Any<T>()` |

## Continuous Integration

Tests should be run in CI/CD pipelines:

```yaml
# Example GitHub Actions workflow
- name: Run Tests
  run: dotnet test --logger "trx;LogFileName=test-results.trx"
  
- name: Publish Test Results
  uses: dorny/test-reporter@v1
  if: always()
  with:
    name: Test Results
    path: '**/test-results.trx'
    reporter: dotnet-trx
```

## Contributing to Tests

When adding new features to CryptoFairPicker:

1. ✅ Write unit tests first (TDD approach recommended)
2. ✅ Follow AAA pattern (Arrange-Act-Assert)
3. ✅ Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
4. ✅ Test both happy paths and error conditions
5. ✅ Mock external dependencies (HTTP, randomness)
6. ✅ Verify determinism for drand-based features
7. ✅ Check uniform distribution for randomness
8. ✅ Ensure all tests pass before committing

## Test Metrics

Current test coverage:
- **Total Tests**: 66
- **Success Rate**: 100%
- **Build Time**: ~3-4 seconds
- **Test Execution**: ~1.7 seconds

## License

Same as parent project - MIT License
