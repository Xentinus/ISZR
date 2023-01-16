using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Migrations
{
    public partial class EditedRequestModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Users_ReuqestForUserId",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "ReuqestForUserId",
                table: "Request",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Request_ReuqestForUserId",
                table: "Request",
                newName: "IX_Request_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Users_UserId",
                table: "Request",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Users_UserId",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Request",
                newName: "ReuqestForUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Request_UserId",
                table: "Request",
                newName: "IX_Request_ReuqestForUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Users_ReuqestForUserId",
                table: "Request",
                column: "ReuqestForUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
