using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatureFlags.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RequireRequirementType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RequirementType",
                table: "FeatureFlag",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "FeatureFlag",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequirementType",
                value: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RequirementType",
                table: "FeatureFlag",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.UpdateData(
                table: "FeatureFlag",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequirementType",
                value: null);
        }
    }
}
