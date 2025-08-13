using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatureFlags.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RenameHiddenToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HiddenToken",
                table: "UserToken",
                newName: "SecondaryToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SecondaryToken",
                table: "UserToken",
                newName: "HiddenToken");
        }
    }
}
