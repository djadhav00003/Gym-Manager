using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRefreshTokenTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
       name: "UserRefreshTokens",
       columns: table => new
       {
           Id = table.Column<int>(nullable: false)
               .Annotation("SqlServer:Identity", "1, 1"),
           UserId = table.Column<int>(nullable: false),
           Token = table.Column<string>(nullable: false),
           JwtId = table.Column<string>(nullable: true),
           IsRevoked = table.Column<bool>(nullable: false),
           CreatedAt = table.Column<DateTime>(nullable: false),
           ExpiresAt = table.Column<DateTime>(nullable: false)
       },
       constraints: table =>
       {
           table.PrimaryKey("PK_UserRefreshTokens", x => x.Id);
           table.ForeignKey(
               name: "FK_UserRefreshTokens_Users_UserId",
               column: x => x.UserId,
               principalTable: "Users",
               principalColumn: "Id",
               onDelete: ReferentialAction.Cascade);
       });

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_UserId",
                table: "UserRefreshTokens",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
       name: "UserRefreshTokens");
        }
    }
}
