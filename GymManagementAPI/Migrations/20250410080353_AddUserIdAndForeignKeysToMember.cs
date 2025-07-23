using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdAndForeignKeysToMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Plans_Gyms_GymId",
                table: "Plans");

            migrationBuilder.DropForeignKey(
                name: "FK_Plans_Gyms_GymId1",
                table: "Plans");

            migrationBuilder.DropForeignKey(
                name: "FK_Plans_Trainers_TrainerId",
                table: "Plans");

            migrationBuilder.DropIndex(
                name: "IX_Plans_GymId",
                table: "Plans");

            migrationBuilder.DropIndex(
                name: "IX_Plans_GymId1",
                table: "Plans");

            migrationBuilder.DropIndex(
                name: "IX_Plans_TrainerId",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "GymId1",
                table: "Plans");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Members",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Members_GymId",
                table: "Members",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_TrainerId",
                table: "Members",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_UserId",
                table: "Members",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Gyms_GymId",
                table: "Members",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Trainers_TrainerId",
                table: "Members",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Users_UserId",
                table: "Members",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Gyms_GymId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_Trainers_TrainerId",
                table: "Members");

            migrationBuilder.DropForeignKey(
                name: "FK_Members_Users_UserId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_GymId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_TrainerId",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_UserId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Members");

            migrationBuilder.AddColumn<int>(
                name: "GymId1",
                table: "Plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_GymId",
                table: "Plans",
                column: "GymId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_GymId1",
                table: "Plans",
                column: "GymId1");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_TrainerId",
                table: "Plans",
                column: "TrainerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_Gyms_GymId",
                table: "Plans",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_Gyms_GymId1",
                table: "Plans",
                column: "GymId1",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Plans_Trainers_TrainerId",
                table: "Plans",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
