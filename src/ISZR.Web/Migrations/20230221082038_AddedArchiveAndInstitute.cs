using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Web.Migrations
{
    public partial class AddedArchiveAndInstitute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Cameras");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(7)",
                oldMaxLength: 7);

            migrationBuilder.AddColumn<int>(
                name: "InsituteId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InstituteId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Group",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Institute",
                columns: table => new
                {
                    InstituteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeaderId = table.Column<int>(type: "int", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institute", x => x.InstituteId);
                    table.ForeignKey(
                        name: "FK_Institute_Users_LeaderId",
                        column: x => x.LeaderId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_InstituteId",
                table: "Users",
                column: "InstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_Institute_LeaderId",
                table: "Institute",
                column: "LeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Institute_InstituteId",
                table: "Users",
                column: "InstituteId",
                principalTable: "Institute",
                principalColumn: "InstituteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Institute_InstituteId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Institute");

            migrationBuilder.DropIndex(
                name: "IX_Users_InstituteId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InsituteId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "InstituteId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Group");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Users",
                type: "nvarchar(48)",
                maxLength: 48,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Cameras",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
