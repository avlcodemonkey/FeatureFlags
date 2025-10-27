using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatureFlags.Domain.Migrations
{
    /// <inheritdoc />
    public partial class CreateApiRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiKeyId = table.Column<int>(type: "INTEGER", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    RequestedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "(current_timestamp)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiRequest", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiRequest_ApiKeyId",
                table: "ApiRequest",
                column: "ApiKeyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiRequest");
        }
    }
}
