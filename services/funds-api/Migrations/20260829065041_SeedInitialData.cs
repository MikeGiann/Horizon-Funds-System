using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Horizon.Funds.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Wallets",
                columns: new[] { "Id", "AccountNumber", "AvailableFunds" },
                values: new object[,]
                {
                    { 1, "", 1000.00m },
                    { 2, "", 500.00m },
                    { 3, "", 0.00m }
                });

            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "Amount", "IdempotencyKey", "Timestamp", "Type", "WalletId" },
                values: new object[,]
                {
                    { 1, 100.00m, "", new DateTime(2026, 1, 1, 10, 0, 0, 0, DateTimeKind.Utc), "Deposit", 1 },
                    { 2, -50.00m, "", new DateTime(2026, 1, 1, 11, 0, 0, 0, DateTimeKind.Utc), "Withdrawal", 1 },
                    { 3, 200.00m, "", new DateTime(2026, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Deposit", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Wallets",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
