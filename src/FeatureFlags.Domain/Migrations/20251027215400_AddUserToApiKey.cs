using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatureFlags.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ApiKey",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ApiKey_UserId",
                table: "ApiKey",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiRequest_ApiKey_ApiKeyId",
                table: "ApiRequest",
                column: "ApiKeyId",
                principalTable: "ApiKey",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiRequest_ApiKey_ApiKeyId",
                table: "ApiRequest");

            migrationBuilder.DropIndex(
                name: "IX_ApiKey_UserId",
                table: "ApiKey");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ApiKey");
        }
    }
}
