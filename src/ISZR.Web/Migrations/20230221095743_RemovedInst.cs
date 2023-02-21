using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Web.Migrations
{
    public partial class RemovedInst : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Institute",
                columns: table => new
                {
                    InstituteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaderId = table.Column<int>(type: "int", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
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
    }
}
