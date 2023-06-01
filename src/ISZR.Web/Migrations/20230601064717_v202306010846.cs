using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Web.Migrations
{
    public partial class v202306010846 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportUserId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSolved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_Users_ReportUserId",
                        column: x => x.ReportUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ReportUserId",
                table: "Reports",
                column: "ReportUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reports");
        }
    }
}
