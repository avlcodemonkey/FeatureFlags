using System.Reflection;
using FeatureFlags.Domain.Attributes;
using FeatureFlags.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Domain.Extensions;

public static class ModelBuilderExtensions {
    /// <summary>
    /// Apply custom DefaultValue and DefaultValueSql attributes.
    /// </summary>
    public static ModelBuilder ApplyDefaults(this ModelBuilder modelBuilder) {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
            foreach (var property in entityType.GetProperties()) {
                var info = property.PropertyInfo ?? property.FieldInfo as MemberInfo;
                if (info == null) {
                    continue;
                }

                var attributes = Attribute.GetCustomAttributes(info);
                if (attributes.Length == 0) {
                    continue;
                }

                if (attributes.FirstOrDefault(x => x is DefaultValueSqlAttribute) is DefaultValueSqlAttribute defaultValueSqlAttr) {
                    property.SetDefaultValueSql(defaultValueSqlAttr.Sql);
                }
                if (attributes.FirstOrDefault(x => x is DefaultValueAttribute) is DefaultValueAttribute defaultValueAttr) {
                    property.SetDefaultValue(defaultValueAttr.DefaultValue);
                }
            }
        }

        return modelBuilder;
    }


    /// <summary>
    /// Seed data to get a clean db up and running.
    /// </summary>
    public static ModelBuilder Seed(this ModelBuilder modelBuilder) {
        var minDate = DateTime.MinValue;

        modelBuilder.Entity<Language>().HasData(
            new Language { Id = 1, Name = "English", LanguageCode = "en", CountryCode = "us", IsDefault = true, CreatedDate = minDate, UpdatedDate = minDate },
            new Language { Id = 2, Name = "Spanish", LanguageCode = "es", CountryCode = "mx", IsDefault = false, CreatedDate = minDate, UpdatedDate = minDate }
        );

        modelBuilder.Entity<Permission>().HasData(
            // hard code controller and action names to avoid a circular reference to the web project
            new Permission { Id = 1, ControllerName = "Dashboard", ActionName = "Index", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 2, ControllerName = "Account", ActionName = "Index", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 4, ControllerName = "Role", ActionName = "Index", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 5, ControllerName = "Role", ActionName = "Edit", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 6, ControllerName = "Role", ActionName = "Delete", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 7, ControllerName = "Role", ActionName = "RefreshPermissions", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 8, ControllerName = "User", ActionName = "Index", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 9, ControllerName = "User", ActionName = "Create", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 10, ControllerName = "User", ActionName = "Edit", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 11, ControllerName = "User", ActionName = "Delete", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 12, ControllerName = "AuditLog", ActionName = "Index", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 13, ControllerName = "AuditLog", ActionName = "View", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 14, ControllerName = "FeatureFlag", ActionName = "Index", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 15, ControllerName = "FeatureFlag", ActionName = "Enable", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 16, ControllerName = "FeatureFlag", ActionName = "Disable", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 17, ControllerName = "FeatureFlag", ActionName = "Create", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 18, ControllerName = "FeatureFlag", ActionName = "ClearCache", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 19, ControllerName = "ApiKey", ActionName = "Index", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 20, ControllerName = "ApiKey", ActionName = "Create", CreatedDate = minDate, UpdatedDate = minDate },
            new Permission { Id = 21, ControllerName = "ApiKey", ActionName = "Delete", CreatedDate = minDate, UpdatedDate = minDate }
        );

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Administrator", IsDefault = true, CreatedDate = minDate, UpdatedDate = minDate }
        );
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission { Id = 1, PermissionId = 1, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 2, PermissionId = 2, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 4, PermissionId = 4, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 5, PermissionId = 5, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 6, PermissionId = 6, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 7, PermissionId = 7, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 8, PermissionId = 8, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 9, PermissionId = 9, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 10, PermissionId = 10, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 11, PermissionId = 11, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 12, PermissionId = 12, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 13, PermissionId = 13, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 14, PermissionId = 14, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 15, PermissionId = 15, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 16, PermissionId = 16, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 17, PermissionId = 17, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 18, PermissionId = 18, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 19, PermissionId = 19, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 20, PermissionId = 20, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate },
            new RolePermission { Id = 21, PermissionId = 21, RoleId = 1, CreatedDate = minDate, UpdatedDate = minDate }
        );

        // enable user registration - feature flag name should match the value in Web.Constants.InternalFeatureFlags
        modelBuilder.Entity<FeatureFlag>().HasData(
            new FeatureFlag { Id = 1, Name = "UserRegistration", IsEnabled = true, CreatedDate = minDate, UpdatedDate = minDate }
        );

        modelBuilder.Entity<ApiKey>().HasData(
            // key is the SHA-512 hash of `replace_me_with_a_real_key`
            new ApiKey { Id = 1, Name = "Default Key", Key = "7edffa1b73ee82ea10a7d18f0d13871bc455175698341b4fe50aabf5d223f8e31b964d86e41d6c1cb41fddf3b1d68290f57372f2b6154d631e6006936b8c96d5", CreatedDate = minDate, UpdatedDate = minDate }
        );

        return modelBuilder;
    }
}
