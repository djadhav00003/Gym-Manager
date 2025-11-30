using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagementAPI.Migrations
{
    /// <inheritdoc />
    public partial class addWebhookKeyToPaymentWebhookLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebhookKey",
                table: "PaymentWebhookLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentWebhookLogs_WebhookKey",
                table: "PaymentWebhookLogs",
                column: "WebhookKey",
                unique: true,
                filter: "[WebhookKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentWebhookLogs_WebhookKey",
                table: "PaymentWebhookLogs");

            migrationBuilder.DropColumn(
                name: "WebhookKey",
                table: "PaymentWebhookLogs");
        }
    }
}
