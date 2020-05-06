using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace coReport.Migrations
{
    public partial class project : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectName",
                table: "Reports");

            migrationBuilder.AddColumn<short>(
                name: "ProjectId",
                table: "Reports",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<short>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(nullable: true),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    IsDone = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ProjectId",
                table: "Reports",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Projects_ProjectId",
                table: "Reports",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Projects_ProjectId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ProjectId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Reports");

            migrationBuilder.AddColumn<string>(
                name: "ProjectName",
                table: "Reports",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
