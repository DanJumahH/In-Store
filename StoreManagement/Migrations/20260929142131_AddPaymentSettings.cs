using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StoreManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add payment columns to Orders ONLY
            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaymentDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNotes",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            // Add Currency to Products
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Products",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "NGN");

            // Create unique index on PaymentReference
            migrationBuilder.CreateIndex(
                name: "IX_Orders_PaymentReference",
                table: "Orders",
                column: "PaymentReference",
                unique: true,
                filter: "[PaymentReference] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_PaymentReference",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PaymentNotes",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Products");
        }
    }
}
