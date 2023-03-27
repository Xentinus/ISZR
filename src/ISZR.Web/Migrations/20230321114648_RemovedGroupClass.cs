using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Web.Migrations
{
    public partial class RemovedGroupClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Classes_ClassId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_ClassId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Groups");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClassId",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ClassId",
                table: "Groups",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Classes_ClassId",
                table: "Groups",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "ClassId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
