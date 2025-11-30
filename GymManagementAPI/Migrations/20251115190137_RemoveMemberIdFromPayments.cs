using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMemberIdFromPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Members_MemberId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_MemberId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Payments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MemberId",
                table: "Payments",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Members_MemberId",
                table: "Payments",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}
