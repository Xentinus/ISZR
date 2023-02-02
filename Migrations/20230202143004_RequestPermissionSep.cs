using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Migrations
{
    public partial class RequestPermissionSep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestedPermissions",
                table: "Requests",
                newName: "WindowsPermissions");

            migrationBuilder.AddColumn<string>(
                name: "FonixPermissions",
                table: "Requests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FonixPermissions",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "WindowsPermissions",
                table: "Requests",
                newName: "RequestedPermissions");
        }
    }
}
