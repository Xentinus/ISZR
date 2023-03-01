﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISZR.Web.Migrations
{
    public partial class AddGenreToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Genre",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genre",
                table: "Users");
        }
    }
}
