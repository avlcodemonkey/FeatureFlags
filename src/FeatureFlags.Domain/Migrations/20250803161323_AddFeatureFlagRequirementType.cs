using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatureFlags.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureFlagRequirementType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RequirementType",
                table: "FeatureFlag",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "FeatureFlag",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequirementType",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequirementType",
                table: "FeatureFlag");
        }
    }
}
