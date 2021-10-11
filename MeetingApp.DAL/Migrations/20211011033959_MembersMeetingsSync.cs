using Microsoft.EntityFrameworkCore.Migrations;

namespace MeetingApp.DAL.Migrations
{
    public partial class MembersMeetingsSync : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dates_Members_memberId",
                table: "Dates");

            migrationBuilder.DropIndex(
                name: "IX_Dates_memberId",
                table: "Dates");

            migrationBuilder.DropColumn(
                name: "memberId",
                table: "Dates");

            migrationBuilder.CreateIndex(
                name: "IX_Members_meetingId",
                table: "Members",
                column: "meetingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Meetings_meetingId",
                table: "Members",
                column: "meetingId",
                principalTable: "Meetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Meetings_meetingId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_meetingId",
                table: "Members");

            migrationBuilder.AddColumn<int>(
                name: "memberId",
                table: "Dates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dates_memberId",
                table: "Dates",
                column: "memberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dates_Members_memberId",
                table: "Dates",
                column: "memberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
