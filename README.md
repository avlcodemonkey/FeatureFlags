# FeatureFlags

A self hosted feature flag solution.  The goal is to provide a simple, performant, and easy to use application for managing feature flags in your applications.  Designed to work with Microsoft's feature management libraries, you can easily integrate with your .NET applications.

## Getting Started

### Requirements

- DotNet SDK 9.0 or later
  - [Download](https://dotnet.microsoft.com/download)
- DotNet Tools
  - Install EF tools globally with `dotnet tool install --global dotnet-ef`
- Node 22 or later
- NPM 10 or later
- SQLite
- (Optional) IDE like Visual Studio or VS Code

### FeatureFlags Setup

Create the SQLite database and seed data using Entity Framework.  Run `dotnet ef database update` in `src/FeatureFlags.Domain` to apply all migrations to the newly created database.

The application uses a feature flag to control access to public user registration.  The `UserRegistration` feature flag is enabled by default, allowing you to create an admin user for your application.  The admin user can then disable the `UserRegistration` feature flag to disable public registration.

Start the application and register yourself as a new user.  You will be assigned the `Administrator` role, which has full access to the application.  You can then log in and manage feature flags, users, and API keys.

To secure your application:
- You can disable the `UserRegistration` feature flag after creating your admin user by editing `appSettings.json`.  This will prevent new users from registering - you'll need to create new users yourself in the UI.
- You may also want to add additional roles with limited permissions for other users to manage feature flags.
- If you leave public registration enabled, new users will be given the default role which is initially set to `Administrator`. Create a new role with limited permissions and set it as the default role for new users.

### Integrating with your Application

The `FeatureFlags.Client` project is a .NET Standard library that can be used in any .NET application.  It provides a `FeatureDefinitionProvider` class that can be used to fetch feature flag definitions from the API.  It also includes a caching mechanism to reduce the number of API calls.

To use the client, register it in your application's dependency injection container.  The client provides an extension method for this using `IHostApplicationBuilder`.  In `Program.cs`:
```csharp
using FeatureFlags.Client;

builder.AddFeatureFlags();
```

You will need to provide the API base URL and an API key for authentication via `appSettings.json`.
```json
{
    "FeatureFlags": {
        "ApiBaseEndpoint": "https://localhost:44308/api/",
        "ApiKey": "[from secrets]",
        "CacheExpirationInMinutes": 15
    }
}
```

The `ApiBaseEndpoint` is the base URL of the API, including the `/api/` suffix.  Make sure to include the trailing slash.  The `ApiKey` is the value of an API key created in the FeatureFlags application.  The `CacheExpirationInMinutes` setting controls how long feature flag definitions are cached in memory.

The `FeatureFlags.Demo` project is a sample ASP.NET Core web application that demonstrates how to use the client library.  It includes examples of how to check feature flag values in views.  Update the appSettings with the correct API base URL and API key to run the demo.

Read more about using Microsoft's feature management functionality at https://docs.azure.cn/en-us/azure-app-configuration/feature-management-dotnet-reference or https://docs.azure.cn/en-us/azure-app-configuration/quickstart-feature-flag-aspnet-core.

## Development

The frontend uses vanilla JavaScript, CSS, and HTML.  No frameworks are used to keep the application lightweight and simple to maintain.  A similar approach is used for the backend, with minimal dependencies.

The solution includes two launch profiles for development:
- `App Debug`: Launches the application only.  This is useful for normal development.
- `Demo Debug`: Launches both the application and demo project.  This is useful for testing changes via the client.

The `src/FeatureFlags.Web/package.json` file defines several commands to help with frontend development, linting, and testing. Run these commands from the `src/FeatureFlags.Web` directory using `npm run <command>`.

| Command      | Description                                                                                                     |
|--------------|-----------------------------------------------------------------------------------------------------------------|
| build        | Builds frontend assets using the custom build script (`build.mjs`).                                             |
| watch        | Runs the build script in watch mode, rebuilding assets on file changes.                                         |
| css:lint     | Lints CSS files in `Assets/css/` using Stylelint and the standard config.                                       |
| css:fix      | Automatically fixes lint errors in CSS files using Stylelint.                                                   |
| test         | Runs JavaScript tests with Node.js test runner and outputs a coverage report. Add ` -- --watch` to watch tests. |
| js:lint      | Lints all JavaScript files in the project using ESLint.                                                         |
| js:fix       | Automatically fixes lint errors in JavaScript files using ESLint.                                               |

## Testing

This project uses Node's built-in test runner for frontend unit tests. Tests live under `src/FeatureFlags.Web/Assets/tests/`. Use one of the approaches below during development.

- Run all frontend tests (from package folder):
  - cd into the package folder and run the npm script:
    - `cd src/FeatureFlags.Web`
    - `npm run test` or `npm test`

- Run a specific test file using the npm script (for convenience, arguments after `--` are forwarded to the Node test runner):
  - From the package folder:
    - `npm run test -- Assets/tests/components/Table.test.js`
  - From repository root using npm's `--prefix` to target the package:
    - `npm run --prefix src/FeatureFlags.Web test -- Assets/tests/components/Table.test.js`

- Run a specific test file directly with Node (no npm script required):
  - From repository root:
    - `node --test src/FeatureFlags.Web/Assets/tests/components/Table.test.js`
  - Or include additional flags (for example, watch mode or verbose reporting):
    - `node --test --watch src/FeatureFlags.Web/Assets/tests/components/Table.test.js`

- Watch mode (rerun tests on file changes):
  - Forward the `--watch` flag to the test runner:
    - `npm run test -- --watch`
    - or `node --test --watch` when running Node directly.

Notes
- Passing a test file path to the npm script works because arguments after `--` are forwarded to the underlying `node --test` invocation defined in the package script.
- These commands require Node 22+ (the project recommends Node 22). If you see unexpected behavior, confirm `node -v` and `npm -v`.
- If a test relies on browser APIs, the test helper `setupDom()` initializes jsdom so tests run deterministically in Node.

## CI

Github actions are used for continuous integration.  The repo is configured to only use verified actions, with minimal permissions, for security.

The workflow runs on pushes and pull requests to the `main` branch.  It checks out the code, sets up .NET and Node.js, installs dependencies, runs linters and tests, and builds the project.  Pull requests must pass all checks before they can be merged.  If all checks succeed when merging into `main`, the version is incremented automatically.

## Logging

- Serilog logging used as it easily integrates with many different sinks.

## To Do

- - Release package to nuget for client project
    - Add instructions for using the package in readme
