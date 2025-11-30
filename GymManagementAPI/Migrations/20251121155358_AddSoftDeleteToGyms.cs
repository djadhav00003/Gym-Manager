using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToGyms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Gyms",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "Gyms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Gyms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Gyms_DeletedByUserId",
                table: "Gyms",
                column: "DeletedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gyms_Users_DeletedByUserId",
                table: "Gyms",
                column: "DeletedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gyms_Users_DeletedByUserId",
                table: "Gyms");

            migrationBuilder.DropIndex(
                name: "IX_Gyms_DeletedByUserId",
                table: "Gyms");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Gyms");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Gyms");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Gyms");
        }
    }
}
