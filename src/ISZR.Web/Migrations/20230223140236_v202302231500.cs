using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Web.Migrations
{
    public partial class v202302231500 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ResolverId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ResolverId",
                table: "Requests",
                column: "ResolverId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Users_ResolverId",
                table: "Requests",
                column: "ResolverId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Users_ResolverId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_ResolverId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ResolverId",
                table: "Requests");
        }
    }
}
