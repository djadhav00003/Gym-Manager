using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentSessionIdToPaymentGatewayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentSessionId",
                table: "PaymentGatewayOrders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentSessionId",
                table: "PaymentGatewayOrders");
        }
    }
}
