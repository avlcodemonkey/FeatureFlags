using FeatureFlags.Models;
using FeatureFlags.Services;
using FeatureFlags.Web.Tests.Fixtures;

namespace FeatureFlags.Web.Tests.Services;

[Collection(nameof(DatabaseCollection))]
public class LanguageServiceTests(DatabaseFixture fixture) {
    private readonly DatabaseFixture _Fixture = fixture;
    private readonly LanguageService _LanguageService = new(fixture.CreateContext());

    [Fact]
    public async Task GetLanguageByIdAsync_ReturnsEnglishLanguageModel() {
        // arrange
        var englishLanguage = await _Fixture.CreateContext().Languages.FindAsync(1);

        // act
        var language = await _LanguageService.GetLanguageByIdAsync(1);

        // assert
        Assert.NotNull(englishLanguage);
        Assert.NotNull(language);
        Assert.IsType<LanguageModel>(language);
        Assert.Equal(englishLanguage.Name, language.Name);
        Assert.True(language.IsDefault);
        Assert.Equal(englishLanguage.LanguageCode, language.LanguageCode);
    }

    [Fact]
    public async Task GetLanguageByIdAsync_WithInvalidLanguageId_ReturnsNull() {
        // arrange
        var languageIdToGet = -200;

        // act
        var result = await _LanguageService.GetLanguageByIdAsync(languageIdToGet);

        // assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllLanguagesAsync_ReturnsTwoLanguageModels() {
        // arrange
        using var context = _Fixture.CreateContext();
        var englishLanguage = await context.Languages.FindAsync(1);
        var spanishLanguage = await context.Languages.FindAsync(2);

        // act
        var languages = await _LanguageService.GetAllLanguagesAsync();

        // assert
        Assert.NotNull(englishLanguage);
        Assert.NotNull(spanishLanguage);
        Assert.NotEmpty(languages);
        Assert.IsType<IEnumerable<LanguageModel>>(languages, exactMatch: false);
        Assert.Equal(2, languages.Count());
        Assert.Collection(languages,
            x => Assert.Equal(englishLanguage.Id, x.Id),
            x => Assert.Equal(spanishLanguage.Id, x.Id)
        );
        Assert.Collection(languages,
            x => Assert.Equal(englishLanguage.Name, x.Name),
            x => Assert.Equal(spanishLanguage.Name, x.Name)
        );
    }
}
