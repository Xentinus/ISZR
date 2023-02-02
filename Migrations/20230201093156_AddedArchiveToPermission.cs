using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Migrations
{
    public partial class AddedArchiveToPermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Classes_ClassId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_ClassId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Requests");

            migrationBuilder.RenameColumn(
                name: "Sector",
                table: "Cameras",
                newName: "Location");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Cameras",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Cameras");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Cameras",
                newName: "Sector");

            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ClassId",
                table: "Requests",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Classes_ClassId",
                table: "Requests",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "ClassId");
        }
    }
}
