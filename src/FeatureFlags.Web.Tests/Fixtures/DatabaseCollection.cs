namespace FeatureFlags.Web.Tests.Fixtures;

/// <summary>
/// See https://xunit.net/docs/shared-context
/// </summary>
[CollectionDefinition(nameof(DatabaseCollection))]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> {
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
