using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Web.Migrations
{
    public partial class v2302221041 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthorizerId",
                table: "Classes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_AuthorizerId",
                table: "Classes",
                column: "AuthorizerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Users_AuthorizerId",
                table: "Classes",
                column: "AuthorizerId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Users_AuthorizerId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_AuthorizerId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "AuthorizerId",
                table: "Classes");
        }
    }
}
