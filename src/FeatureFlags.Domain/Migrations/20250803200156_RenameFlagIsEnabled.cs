using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatureFlags.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RenameFlagIsEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "FeatureFlag");

            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "FeatureFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "FeatureFlag",
                keyColumn: "Id",
                keyValue: 1,
                column: "Status",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "FeatureFlag");

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "FeatureFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "FeatureFlag",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsEnabled",
                value: true);
        }
    }
}
