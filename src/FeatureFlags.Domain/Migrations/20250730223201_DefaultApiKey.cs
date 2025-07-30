using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatureFlags.Domain.Migrations
{
    /// <inheritdoc />
    public partial class DefaultApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ApiKey",
                columns: new[] { "Id", "Key", "Name" },
                values: new object[] { 1, "7edffa1b73ee82ea10a7d18f0d13871bc455175698341b4fe50aabf5d223f8e31b964d86e41d6c1cb41fddf3b1d68290f57372f2b6154d631e6006936b8c96d5", "Default Key" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ApiKey",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
