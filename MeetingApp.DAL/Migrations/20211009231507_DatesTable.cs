using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MeetingApp.DAL.Migrations
{
    public partial class DatesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dates_Meetings_MeetingId",
                table: "Dates");

            migrationBuilder.RenameColumn(
                name: "MeetingId",
                table: "Dates",
                newName: "meetingId");

            migrationBuilder.RenameIndex(
                name: "IX_Dates_MeetingId",
                table: "Dates",
                newName: "IX_Dates_meetingId");

            migrationBuilder.AlterColumn<Guid>(
                name: "meetingId",
                table: "Dates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Dates_Meetings_meetingId",
                table: "Dates",
                column: "meetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dates_Meetings_meetingId",
                table: "Dates");

            migrationBuilder.RenameColumn(
                name: "meetingId",
                table: "Dates",
                newName: "MeetingId");

            migrationBuilder.RenameIndex(
                name: "IX_Dates_meetingId",
                table: "Dates",
                newName: "IX_Dates_MeetingId");

            migrationBuilder.AlterColumn<Guid>(
                name: "MeetingId",
                table: "Dates",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Dates_Meetings_MeetingId",
                table: "Dates",
                column: "MeetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
