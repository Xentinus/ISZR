using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Migrations
{
    public partial class FixedRequestModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Users_UserId",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_Request_UserId",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Request",
                newName: "RequestAuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_AuthorId",
                table: "Request",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_RequestForId",
                table: "Request",
                column: "RequestForId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Users_AuthorId",
                table: "Request",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Users_RequestForId",
                table: "Request",
                column: "RequestForId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Users_AuthorId",
                table: "Request");

            migrationBuilder.DropForeignKey(
                name: "FK_Request_Users_RequestForId",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_Request_AuthorId",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_Request_RequestForId",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "RequestAuthorId",
                table: "Request",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_UserId",
                table: "Request",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Users_UserId",
                table: "Request",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
