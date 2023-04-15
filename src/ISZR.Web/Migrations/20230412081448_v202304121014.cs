using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Web.Migrations
{
    public partial class v202304121014 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cameras",
                columns: table => new
                {
                    CameraId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cameras", x => x.CameraId);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    WindowsPermissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FonixPermissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.GroupId);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActiveDirectoryPermissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.PermissionId);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    ClassId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AuthorizerId = table.Column<int>(type: "int", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.ClassId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Rank = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Genre = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogonCount = table.Column<int>(type: "int", nullable: false),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Users_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Parkings",
                columns: table => new
                {
                    ParkingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Modell = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LicensePlate = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    OwnerUserId = table.Column<int>(type: "int", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parkings", x => x.ParkingId);
                    table.ForeignKey(
                        name: "FK_Parkings_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Phones",
                columns: table => new
                {
                    PhoneId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    PhoneUserId = table.Column<int>(type: "int", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phones", x => x.PhoneId);
                    table.ForeignKey(
                        name: "FK_Phones_Users_PhoneUserId",
                        column: x => x.PhoneUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WindowsPermissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FonixPermissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CarId = table.Column<int>(type: "int", nullable: true),
                    PhoneId = table.Column<int>(type: "int", nullable: true),
                    CreatedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedForUserId = table.Column<int>(type: "int", nullable: true),
                    ClosedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_Requests_Parkings_CarId",
                        column: x => x.CarId,
                        principalTable: "Parkings",
                        principalColumn: "ParkingId");
                    table.ForeignKey(
                        name: "FK_Requests_Phones_PhoneId",
                        column: x => x.PhoneId,
                        principalTable: "Phones",
                        principalColumn: "PhoneId");
                    table.ForeignKey(
                        name: "FK_Requests_Users_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Requests_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_Requests_Users_CreatedForUserId",
                        column: x => x.CreatedForUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_AuthorizerId",
                table: "Classes",
                column: "AuthorizerId");

            migrationBuilder.CreateIndex(
                name: "IX_Parkings_OwnerUserId",
                table: "Parkings",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Phones_PhoneUserId",
                table: "Phones",
                column: "PhoneUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CarId",
                table: "Requests",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ClosedByUserId",
                table: "Requests",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CreatedByUserId",
                table: "Requests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CreatedForUserId",
                table: "Requests",
                column: "CreatedForUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_PhoneId",
                table: "Requests",
                column: "PhoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClassId",
                table: "Users",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PositionId",
                table: "Users",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Users_AuthorizerId",
                table: "Classes",
                column: "AuthorizerId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Users_AuthorizerId",
                table: "Classes");

            migrationBuilder.DropTable(
                name: "Cameras");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Parkings");

            migrationBuilder.DropTable(
                name: "Phones");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Positions");
        }
    }
}
