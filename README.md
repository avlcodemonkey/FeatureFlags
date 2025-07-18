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
