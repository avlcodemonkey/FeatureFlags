# GitHub Copilot Instructions

## Project Overview
This repository is a .NET 10 MVC solution for feature flag management. It includes:
- MVC web UI and API controllers.
- Client library for remote feature flag definitions.
- Domain and service layers for feature flag persistence and business logic.

## Coding Style
- Use C# 14 features and idioms.
- Prefer async/await for asynchronous operations.
- Use dependency injection for all services and clients.
- Follow conventions from the editorconfig file.
- Use records for immutable models where appropriate.
- Use sealed classes where inheritance is not needed.
- Favor simple, expressive code over complex constructs.
- Keep performance in mind, but prioritize readability and maintainability.
- Keep code comments concise and relevant. Don't start comments with an article like "a", "an", or "the".

## MVC Guidance
- Prioritize MVC patterns over Blazor or Razor Pages.
- For API endpoints, use `[ApiController]` and attribute routing.

## Testing
- Use xUnit for unit tests.
- Use Moq for mocking dependencies.
- Test both success and error paths for controllers and services.
- Place tests in the corresponding `*.Tests` projects.

## Feature Flags
- Feature flag definitions are managed via the `FeatureFlagModel` and related services.
- The client library (`IFeatureFlagClient`, `HttpFeatureFlagClient`) fetches definitions from a remote API.
- Controllers expose endpoints for listing, enabling, disabling, and clearing feature flag cache.

## Extensibility
- Add new API endpoints in controllers following the existing attribute routing style.
- Use extension methods for reusable logic.

## General Guidelines
- Keep controllers thin; delegate business logic to services.
- Use resource files for user-facing messages and error strings.
- Document public APIs with XML comments.
- Use cancellation tokens for async service methods.

## Contribution
- Follow the existing folder and namespace structure.
- Write unit tests for all new features.
- Ensure code builds and tests pass on .NET 10.
