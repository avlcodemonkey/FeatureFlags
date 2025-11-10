using System.Reflection;
using FeatureFlags.Client;
using FeatureFlags.Constants;
using FeatureFlags.Domain;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;

namespace FeatureFlags.Extensions.Program;

/// <summary>
/// Provides extension methods for registering custom services and dependencies in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions {
    /// <summary>
    /// Registers dbContext and application specific services.
    /// </summary>
    public static IServiceCollection AddCustomServices(this IServiceCollection services) {
        services.AddDbContext<FeatureFlagsDbContext>();

        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IApiRequestService, ApiRequestService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IEmailService, MailtrapService>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IViewService, ViewService>();

        services.AddScoped<ApiKeyAuthenticationHandler>();

        return services;
    }

    /// <summary>
    /// Register response compression with custom settings.
    /// </summary>
    public static IServiceCollection AddCustomResponseCompression(this IServiceCollection services) {
        services.AddResponseCompression(options => {
            // enable compression only for assets
            options.EnableForHttps = true;
            options.MimeTypes = new List<string>() { "text/css", "application/javascript", "text/javascript", "font/woff2" };
        });

        return services;
    }

    /// <summary>
    /// Register swagger dependencies.
    /// </summary>
    public static IServiceCollection AddSwagger(this IServiceCollection services) {
        // Register Swagger limited to the public API endpoints group
        services.AddSwaggerGen(options => {
            options.DocInclusionPredicate((_, api) => api.GroupName == Swagger.GroupName);
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

            options.AddSecurityDefinition(ApiKeyAuthenticationOptions.AuthenticationScheme, new OpenApiSecurityScheme {
                In = ParameterLocation.Header,
                Name = Client.Constants.ApiKeyHeaderName,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = ApiKeyAuthenticationOptions.AuthenticationScheme }
            }, Array.Empty<string>() } });
        });
        return services;
    }

    /// <summary>
    /// Sets up feature flag services using Microsoft.FeatureManagement and appSettings configuration.
    /// </summary>
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services) {
        // configures the feature management dependencies include filters for PercentageFilter, TimeWindowFilter,ContextualTargetingFilter and TargetingFilter
        // register any custom filter types here also so they can be used when viewing/editing feature flags
        services.AddScopedFeatureManagement()
            .AddFeatureFilter<ConsistentPercentageFilter>()
            .WithTargeting();

        services.Configure<ConfigurationFeatureDefinitionProviderOptions>(x => x.CustomConfigurationMergingEnabled = true);

        return services;
    }
}
