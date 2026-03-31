using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pet.Jira.Infrastructure.Migrations
{
    public partial class Add_UserCalendarConnections : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserCalendarConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AccessTokenProtected = table.Column<string>(type: "TEXT", nullable: true),
                    RefreshTokenProtected = table.Column<string>(type: "TEXT", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Scope = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalAccountId = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalLogin = table.Column<string>(type: "TEXT", nullable: true),
                    LastConnectedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastRefreshAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastError = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCalendarConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCalendarConnections_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCalendarConnections_UserId_Provider",
                table: "UserCalendarConnections",
                columns: new[] { "UserId", "Provider" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCalendarConnections");
        }
    }
}
