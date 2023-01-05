using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Migrations
{
    public partial class EditedUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableLogin",
                table: "User",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableLogin",
                table: "User");
        }
    }
}
