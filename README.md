# FeatureFlags

A self hosted feature flag solution.

## Getting Started

### Requirements

- Visual Studio is required for building and running the application.

#### Create

Create the database schema and seed data using Entity Framework.  Run `dotnet ef database update` to apply all migrations to the newly created database.

## CI

Github actions are used for continuous integration.  The repo is configured to only use verified actions, with minimal permissions, for security.  There is some room for improvement.  Since this is a private repo but not part of an organization, it isn't allowed to have branch protection rules.  There's also no pretty reporting for test results.

## Logging

- Console logging used as it easily integrates with cloud hosting providers.

## To Do

- Add API endpoints for fetching feature flag configuration
    - ~~Get all feature flags~~
    - ~~Get specific feature flag by name~~
    - Get feature flag for user
    - GET requests authenticated with API key
    - Add tests for new API endpoints
- Add API key support for authentication
    - UI to create new API keys
    - New auth attribute to validate API keys
    - Add tests for API key authentication
- ~~Move FeatureDefinitionProvider to client project.~~
    - ~~Add tests for client project.~~
- Create client service for consuming feature flag API in client project with FeatureDefinitionProvider.
    - Methods to
        - ~~Get all feature flags~~
        - ~~Get specific feature flag by name~~
        - Get feature flag value for user.
    - ~~Add caching for feature flag definitions in client, configurable with appSettings. Default 15 minutes.~~
    - ~~Add a clear cache method.~~
    - ~~Add appSettings for cache lifetime.~~
- ~~Add extension method in client project to register FeatureDefinitionProvider, client service, IMemoryCache, and httpClient.~~
    - ~~Use auth for httpClient. Add header with API key.~~
    - ~~Add api key in appsettings.~~
- ~~Use new feature definition provider in web project.~~
- Add UI support for filters
    - User can pick one type of filter for each feature flag?
    - Percentage of users - just pick a percentage
    - Time window - pick start and end time
    - Targeting - set patterns to include or exclude users like `*@domain.com`
- Persistent percentage custom filter
    - Like built in percentage but saves user to database so they always get the same value
    - Build UI to support this
    - Add get flag for user API endpoint
        - return default value if user claim is not found (without calling api)
        - if user is not found in db, will set value for user and save to database
    - Does changing flag config reset existing users?
- Build out readme to explain how to use the feature flag solution
- Release package to nuget for client project
    - Add instructions for using the package in readme
