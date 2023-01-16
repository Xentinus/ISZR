using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Migrations
{
    public partial class FixedRequestAuthor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Users_AuthorId",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_Request_AuthorId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Request");

            migrationBuilder.CreateIndex(
                name: "IX_Request_RequestAuthorId",
                table: "Request",
                column: "RequestAuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Users_RequestAuthorId",
                table: "Request",
                column: "RequestAuthorId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Users_RequestAuthorId",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_Request_RequestAuthorId",
                table: "Request");

            migrationBuilder.AddColumn<int>(
                name: "AuthorId",
                table: "Request",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Request_AuthorId",
                table: "Request",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Users_AuthorId",
                table: "Request",
                column: "AuthorId",
                principalTable: "Users",
                principalColumn: "UserId");
        }
    }
}
