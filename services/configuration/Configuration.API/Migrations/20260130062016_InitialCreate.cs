using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Configuration.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "configuration");

            migrationBuilder.CreateTable(
                name: "feature_flags",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feature_flags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organization_settings",
                schema: "configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Value = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_feature_flags_OrganizationId_Key",
                schema: "configuration",
                table: "feature_flags",
                columns: new[] { "OrganizationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_settings_OrganizationId_Key",
                schema: "configuration",
                table: "organization_settings",
                columns: new[] { "OrganizationId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "feature_flags",
                schema: "configuration");

            migrationBuilder.DropTable(
                name: "organization_settings",
                schema: "configuration");
        }
    }
}
