using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerToPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
