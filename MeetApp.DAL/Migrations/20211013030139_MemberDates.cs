using Microsoft.EntityFrameworkCore.Migrations;

namespace MeetApp.DAL.Migrations
{
    public partial class MemberDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "memberId",
                table: "Dates",
                newName: "userId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "userId",
                table: "Dates",
                newName: "memberId");
        }
    }
}
