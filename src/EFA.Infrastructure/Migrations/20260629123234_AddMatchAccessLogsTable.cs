using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchAccessLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchAccessLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MemberCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScannedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsAllowed = table.Column<bool>(type: "bit", nullable: false),
                    RejectionReasonCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RejectionReasonName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssignmentRoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PermissionText = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ScannedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchAccessLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchAccessLogs_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchAccessLogs_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchAccessLogs_MatchId",
                table: "MatchAccessLogs",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchAccessLogs_MatchId_MemberId_IsAllowed",
                table: "MatchAccessLogs",
                columns: new[] { "MatchId", "MemberId", "IsAllowed" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchAccessLogs_MemberId",
                table: "MatchAccessLogs",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchAccessLogs_ScannedAt",
                table: "MatchAccessLogs",
                column: "ScannedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchAccessLogs");
        }
    }
}
