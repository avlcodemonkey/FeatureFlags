# FeatureFlags

A self hosted feature flag solution.

## Getting Started

### Requirements

- Visual Studio is required for building and running the application.

#### Setup

Create the SQLite database and seed data using Entity Framework.  Run `dotnet ef database update` in `src/FeatureFlags.Domain` to apply all migrations to the newly created database.

The application uses a feature flag to control access to public user registration.  The `UserRegistration` feature flag is enabled by default, allowing you to create an admin user for your application.  The admin user can then disable the `UserRegistration` feature flag to disable public registration.

A default API key is created named `Default Key` with the value `replace_me_with_a_real_key`.  Add that key to your appSettings or user secrets to use the API key authentication.  You need this to enable user registration.
```json
    "FeatureFlags": {
        "ApiKey": "replace_me_with_a_real_key"
    }
```

Start the application and register yourself as a new user.  You will be assigned the `Administrator` role, which has full access to the application.  You can then log in and manage feature flags, users, and API keys.

To secure your application:
- After creating your admin user, delete the `Default Key` API key and create a new one with a secure value.  The default key is only for initial setup.  Update your appSettings or user secrets with the new key value.
- You can disable the `UserRegistration` feature flag after creating your admin user.  This will prevent new users from registering - you'll need to create new users yourself in the UI.
- You may also want to add additional roles with limited permissions for other users to manage feature flags.
- If you leave public registration enabled, new users will be given the default role which is initially set to `Administrator`. Create a new role with limited permissions and set it as the default role for new users.

## CI

Github actions are used for continuous integration.  The repo is configured to only use verified actions, with minimal permissions, for security.

## Logging

- Console logging used as it easily integrates with cloud hosting providers.

## To Do

- Add API endpoints for fetching feature flag configuration
    - ~~Get all feature flags~~
    - ~~Get specific feature flag by name~~
    - Get feature flag for user, with unit tests
    - ~~GET requests authenticated with API key~~
- ~~Add API key support for authentication~~
    - ~~UI to create new API keys~~
    - ~~New auth attribute to validate API keys~~
    - ~~Add tests for API key authentication~~
- ~~Move FeatureDefinitionProvider to client project.~~
    - ~~Add tests for client project.~~
- Create client service for consuming feature flag API in client project with FeatureDefinitionProvider.
    - Methods to
        - ~~Get all feature flags~~
        - ~~Get specific feature flag by name~~
        - Get feature flag value for user, with unit tests
    - ~~Add caching for feature flag definitions in client, configurable with appSettings. Default 15 minutes.~~
    - ~~Add a clear cache method.~~
    - ~~Add appSettings for cache lifetime.~~
- ~~Add extension method in client project to register FeatureDefinitionProvider, client service, IMemoryCache, and httpClient.~~
    - ~~Use auth for httpClient. Add header with API key.~~
    - ~~Add api key in appsettings.~~
- ~~Use new feature definition provider in web project.~~
- Add UI support for filters
    - Users can add as many filters as they want
    - Percentage of users - just pick a percentage using a range input
    - Time window - pick start and end time, recurrence
    - Targeting - set patterns to include or exclude users like `email@domain.com`
        - Allow targeting users by email or group/role
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
- Add serilog with appSetting configuration for structured logging
