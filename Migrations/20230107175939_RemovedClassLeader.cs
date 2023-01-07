using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Migrations
{
    public partial class RemovedClassLeader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeaderName",
                table: "Class");

            migrationBuilder.DropColumn(
                name: "LeaderRank",
                table: "Class");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeaderName",
                table: "Class",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LeaderRank",
                table: "Class",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
