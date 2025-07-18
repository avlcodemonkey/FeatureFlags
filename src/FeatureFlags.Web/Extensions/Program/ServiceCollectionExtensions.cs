using FeatureFlags.Domain;
using FeatureFlags.Services;
using FeatureFlags.Utils;
using Microsoft.FeatureManagement;

namespace FeatureFlags.Extensions.Program;

public static class ServiceCollectionExtensions {
    /// <summary>
    /// Registers dbContext and application specific services.
    /// </summary>
    public static IServiceCollection AddCustomServices(this IServiceCollection services) {
        services.AddDbContext<FeatureFlagsDbContext>();

        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IEmailService, MailtrapService>();
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IViewService, ViewService>();

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
    /// Register feature flag dependencies.
    /// </summary>
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services) {
        services.AddScoped<IFeatureDefinitionProvider, DbFeatureDefinitionProvider>()
            .AddScopedFeatureManagement();

        return services;
    }
}
