# Changelog

All notable changes to CryptoFairPicker will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-11-07

### Added
- **New drand-based API** with `IWinnerSelector` and `IFairRandomSource` interfaces
- `RoundId` record type for identifying randomness rounds
- `DrandRandomSource` implementation fetching randomness from drand HTTP API
- `DrandWinnerSelector` implementation with rejection sampling for uniform distribution
- `CsprngRandomSource` and `CsprngWinnerSelector` as fast local fallback options
- `DrandOptions` configuration class with support for BaseUrl, Chain, TimeoutSeconds, and RetryCount
- Dependency injection extensions:
  - `AddCryptoFairPickerDrand(IConfiguration)` - Configure from appsettings.json
  - `AddCryptoFairPickerDrand(Action<DrandOptions>)` - Configure programmatically
  - `AddCryptoFairPickerCsprng()` - Use CSPRNG fallback
  - `AddCryptoFairPicker(IConfiguration)` - Strategy selection via configuration
- Configuration support via `appsettings.json` with strategy selector
- Polly-based retry policies for transient network failures
- HttpClientFactory integration for proper HTTP client lifecycle management
- Comprehensive test suite with 66 passing tests
- Sample console application demonstrating drand integration
- EditorConfig for consistent code style
- Documentation:
  - Updated README.md focused on drand
  - CHANGELOG.md
  - docs/VERIFY.md with verification steps

### Changed
- **Default strategy is now drand** instead of CSPRNG
- Winners are now **1-indexed** (range [1, n]) instead of 0-indexed in the new API
- Upgraded to use deterministic round-based selection instead of immediate randomness
- Added nullable reference types throughout (already enabled)
- Updated package dependencies:
  - Added `Polly` 8.5.0 for retry logic
  - Added `Microsoft.Extensions.Configuration.Binder` 9.0.10
  - Added `Microsoft.Extensions.Options.ConfigurationExtensions` 9.0.10

### Maintained
- Existing `IPickerStrategy` and `IFairPicker` interfaces remain available for backward compatibility
- All existing strategies (CSPRNG, Commit-Reveal, DrandBeacon) continue to work
- All existing tests pass (37 original tests + 29 new tests = 66 total)
- No breaking changes to existing API

### Security
- Implemented proper error handling for network failures
- Added timeouts to prevent hanging requests
- Retry logic with exponential backoff for resilience
- SHA-256 hashing of drand randomness for additional entropy
- Rejection sampling to avoid modulo bias in random number generation
- HttpClientFactory usage to prevent socket exhaustion

## [1.0.0] - Previous Release

Initial release with:
- CSPRNG strategy using RandomNumberGenerator
- Commit-Reveal strategy for auditable draws
- Basic drand beacon support
- Dependency injection support
- Uniform distribution without modulo bias
