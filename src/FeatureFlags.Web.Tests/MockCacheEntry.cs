using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace FeatureFlags.Web.Tests;

/// <summary>
/// Mock implementation of <see cref="ICacheEntry"/> for unit testing.
/// </summary>
internal class MockCacheEntry : ICacheEntry {
    private bool _Disposed = false;

    public DateTimeOffset? AbsoluteExpiration { get; set; }
    public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

    public IList<IChangeToken> ExpirationTokens => throw new NotImplementedException();

    public required object Key { get; set; }

    public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks => throw new NotImplementedException();

    public CacheItemPriority Priority { get; set; }
    public long? Size { get; set; }
    public TimeSpan? SlidingExpiration { get; set; }
    public object? Value { get; set; }

    protected virtual void Dispose(bool disposing) {
        if (!_Disposed) {
            if (disposing) {
                // Dispose managed resources here.
            }
            // Dispose unmanaged resources here.
            _Disposed = true;
        }
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
