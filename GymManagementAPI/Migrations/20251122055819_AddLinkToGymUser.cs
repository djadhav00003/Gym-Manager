using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkToGymUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerUserId",
                table: "Gyms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Gyms_OwnerUserId",
                table: "Gyms",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gyms_Users_OwnerUserId",
                table: "Gyms",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gyms_Users_OwnerUserId",
                table: "Gyms");

            migrationBuilder.DropIndex(
                name: "IX_Gyms_OwnerUserId",
                table: "Gyms");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Gyms");
        }
    }
}
