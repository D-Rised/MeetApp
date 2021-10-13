using Microsoft.EntityFrameworkCore.Migrations;

namespace MeetApp.DAL.Migrations
{
    public partial class MemberDatesList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Dates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dates_MemberId",
                table: "Dates",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dates_Members_MemberId",
                table: "Dates",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dates_Members_MemberId",
                table: "Dates");

            migrationBuilder.DropIndex(
                name: "IX_Dates_MemberId",
                table: "Dates");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Dates");
        }
    }
}
