using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EFA.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentRoleNameToAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignmentRoleName",
                table: "Assignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentRoleName",
                table: "Assignments");
        }
    }
}
