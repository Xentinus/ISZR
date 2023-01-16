using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Migrations
{
    public partial class AddedUserSelectForRequest : Migration
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
                name: "Description",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "RequestFor",
                table: "Request");

            migrationBuilder.AddColumn<int>(
                name: "LogonCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequestForId",
                table: "Request",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReuqestForUserId",
                table: "Request",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Request_ReuqestForUserId",
                table: "Request",
                column: "ReuqestForUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Users_ReuqestForUserId",
                table: "Request",
                column: "ReuqestForUserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Users_ReuqestForUserId",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_Request_ReuqestForUserId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "LogonCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RequestForId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "ReuqestForUserId",
                table: "Request");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Request",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestFor",
                table: "Request",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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
