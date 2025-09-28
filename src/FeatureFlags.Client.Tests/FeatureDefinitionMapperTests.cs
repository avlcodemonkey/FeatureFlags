using Microsoft.FeatureManagement;

namespace FeatureFlags.Client.Tests;

public class FeatureDefinitionMapperTests {
    [Fact]
    public void ToFeatureDefinition_ReturnsDefinition() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature name",
            EnabledFor = [new CustomFeatureFilterConfiguration { Name = "filter name" }],
            Variants = [new VariantDefinition { Name = "variant name" }],
        };

        var definition = FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.NotNull(definition);
        Assert.Equal(customFeatureDefinition.Name, definition.Name);
        Assert.NotNull(definition.EnabledFor);
        Assert.Single(definition.EnabledFor);
        Assert.Equal("filter name", definition.EnabledFor.First().Name);
        Assert.NotNull(definition.Variants);
        Assert.Single(definition.Variants);
        Assert.Equal("variant name", definition.Variants.First().Name);
    }

    [Fact]
    public void ToFeatureDefinition_WithEmptyEnabledFor_ReturnsDefinition() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature name"
        };

        var definition = FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.NotNull(definition);
        Assert.Equal(customFeatureDefinition.Name, definition.Name);
        Assert.NotNull(definition.EnabledFor);
        Assert.Empty(definition.EnabledFor);
    }

    [Fact]
    public void ToFeatureDefinition_WithMultipleEnabledFor_ReturnsAllFilters() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature name",
            EnabledFor =
            [
                new CustomFeatureFilterConfiguration { Name = "filter1" },
                new CustomFeatureFilterConfiguration { Name = "filter2" }
            ]
        };

        var definition = FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.NotNull(definition);
        Assert.Equal(customFeatureDefinition.Name, definition.Name);
        Assert.NotNull(definition.EnabledFor);
        Assert.Equal(2, definition.EnabledFor.Count());
        Assert.Contains(definition.EnabledFor, f => f.Name == "filter1");
        Assert.Contains(definition.EnabledFor, f => f.Name == "filter2");
    }

    [Fact]
    public void ToFeatureDefinition_WithNullInput_ThrowsArgumentNullException()
        => Assert.Throws<NullReferenceException>(() => FeatureDefinitionMapper.ToFeatureDefinition(null!));

    [Fact]
    public void ToFeatureDefinition_WithNullName_MapsToNullName() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = null!
        };

        var definition = FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.NotNull(definition);
        Assert.Null(definition.Name);
    }

    [Fact]
    public void ToFeatureDefinition_WithEmptyName_MapsToEmptyName() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = ""
        };

        var definition = FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.NotNull(definition);
        Assert.Equal("", definition.Name);
    }

    [Fact]
    public void ToFeatureDefinition_WithNullEnabledFor_ReturnsNullEnabledFor() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature",
            EnabledFor = null!
        };

        var definition = FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.NotNull(definition);
        Assert.Null(definition.EnabledFor);
    }

    [Fact]
    public void ToFeatureDefinition_WithNullVariants_ReturnsEmptyVariants() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature",
            Variants = null!
        };

        var definition = FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.NotNull(definition);
        Assert.NotNull(definition.Variants);
        Assert.Empty(definition.Variants);
    }

    [Fact]
    public void ToFeatureDefinition_WithMultipleVariants_ReturnsAllVariants() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature",
            Variants =
            [
                new VariantDefinition { Name = "variant1" },
                new VariantDefinition { Name = "variant2" }
            ]
        };

        var definition = FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.NotNull(definition);
        Assert.NotNull(definition.Variants);
        Assert.Equal(2, definition.Variants.Count());
        Assert.Contains(definition.Variants, v => v.Name == "variant1");
        Assert.Contains(definition.Variants, v => v.Name == "variant2");
    }

    [Fact]
    public void ToFeatureDefinition_WithFilterWithNullName_MapsToNullName() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature",
            EnabledFor = [new CustomFeatureFilterConfiguration { Name = null! }]
        };

        var definition = FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.NotNull(definition);
        Assert.Single(definition.EnabledFor);
        Assert.Null(definition.EnabledFor.First().Name);
    }

    [Fact]
    public void ToFeatureDefinition_WithVariantWithNullName_MapsToNullName() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature",
            Variants = [new VariantDefinition { Name = null }]
        };

        var definition = FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.NotNull(definition);
        Assert.Single(definition.Variants);
        Assert.Null(definition.Variants.First().Name);
    }

    [Fact]
    public void ToFeatureDefinition_DoesNotMutateInput() {
        var customFeatureDefinition = new CustomFeatureDefinition {
            Name = "feature",
            EnabledFor = [new CustomFeatureFilterConfiguration { Name = "filter" }],
            Variants = [new VariantDefinition { Name = "variant" }]
        };

        var originalName = customFeatureDefinition.Name;
        var originalEnabledFor = customFeatureDefinition.EnabledFor.ToArray();
        var originalVariants = customFeatureDefinition.Variants.ToArray();

        FeatureDefinitionMapper.ToFeatureDefinition(customFeatureDefinition);

        Assert.Equal(originalName, customFeatureDefinition.Name);
        Assert.Equal(originalEnabledFor, customFeatureDefinition.EnabledFor);
        Assert.Equal(originalVariants, customFeatureDefinition.Variants);
    }
}
