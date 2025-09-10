using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeatureFlags.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddFeatureFlagFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeatureFlagFilters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeatureFlagId = table.Column<int>(type: "INTEGER", nullable: false),
                    FilterType = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeStart = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    TimeEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    TimeRecurrenceType = table.Column<int>(type: "INTEGER", nullable: true),
                    TimeRecurrenceInterval = table.Column<int>(type: "INTEGER", nullable: true),
                    TimeRecurrenceDaysOfWeek = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TimeRecurrenceFirstDayOfWeek = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TimeRecurrenceRangeType = table.Column<int>(type: "INTEGER", nullable: true),
                    TimeRecurrenceEndDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    TimeRecurrenceNumberOfOccurrences = table.Column<int>(type: "INTEGER", nullable: true),
                    PercentageValue = table.Column<int>(type: "INTEGER", nullable: true),
                    JSON = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "(current_timestamp)"),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "(current_timestamp)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlagFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureFlagFilters_FeatureFlag_FeatureFlagId",
                        column: x => x.FeatureFlagId,
                        principalTable: "FeatureFlag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeatureFlagFilterUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FeatureFlagFilterId = table.Column<int>(type: "INTEGER", nullable: false),
                    User = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Include = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "(current_timestamp)"),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "(current_timestamp)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlagFilterUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureFlagFilterUsers_FeatureFlagFilters_FeatureFlagFilterId",
                        column: x => x.FeatureFlagFilterId,
                        principalTable: "FeatureFlagFilters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagFilters_FeatureFlagId",
                table: "FeatureFlagFilters",
                column: "FeatureFlagId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagFilterUsers_FeatureFlagFilterId",
                table: "FeatureFlagFilterUsers",
                column: "FeatureFlagFilterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeatureFlagFilterUsers");

            migrationBuilder.DropTable(
                name: "FeatureFlagFilters");
        }
    }
}
