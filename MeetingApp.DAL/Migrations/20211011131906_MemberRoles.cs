using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MeetingApp.DAL.Migrations
{
    public partial class MemberRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ownerId",
                table: "Meetings");

            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role",
                table: "Members");

            migrationBuilder.AddColumn<Guid>(
                name: "ownerId",
                table: "Meetings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
