using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace coReport.Migrations
{
    public partial class project2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDone",
                table: "Projects");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Projects",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Projects");

            migrationBuilder.AddColumn<bool>(
                name: "IsDone",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
