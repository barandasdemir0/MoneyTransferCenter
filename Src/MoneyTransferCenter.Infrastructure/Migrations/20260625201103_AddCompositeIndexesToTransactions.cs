using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoneyTransferCenter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexesToTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_ReceiverAccountId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SenderAccountId",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReceiverAccountId_CreatedAt",
                table: "Transactions",
                columns: new[] { "ReceiverAccountId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SenderAccountId_CreatedAt",
                table: "Transactions",
                columns: new[] { "SenderAccountId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_ReceiverAccountId_CreatedAt",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_SenderAccountId_CreatedAt",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ReceiverAccountId",
                table: "Transactions",
                column: "ReceiverAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SenderAccountId",
                table: "Transactions",
                column: "SenderAccountId");
        }
    }
}
