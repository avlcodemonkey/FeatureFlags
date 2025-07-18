using FeatureFlags.Domain;
using FeatureFlags.Extensions.Services;
using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Services;

public sealed class LanguageService(FeatureFlagsDbContext dbContext) : ILanguageService {
    private readonly FeatureFlagsDbContext _DbContext = dbContext;

    public async Task<IEnumerable<LanguageModel>> GetAllLanguagesAsync(CancellationToken cancellationToken = default)
        => await _DbContext.Languages.AsNoTracking().SelectAsModel().ToListAsync(cancellationToken);

    public async Task<LanguageModel?> GetLanguageByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _DbContext.Languages.AsNoTracking().Where(x => x.Id == id).SelectAsModel().FirstOrDefaultAsync(cancellationToken);
}
