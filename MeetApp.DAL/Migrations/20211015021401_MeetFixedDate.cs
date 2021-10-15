using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MeetApp.DAL.Migrations
{
    public partial class MeetFixedDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dateFinal",
                table: "Meets");

            migrationBuilder.AddColumn<bool>(
                name: "fixedDate",
                table: "Meets",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fixedDate",
                table: "Meets");

            migrationBuilder.AddColumn<DateTime>(
                name: "dateFinal",
                table: "Meets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
