using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GCTWeb.Migrations
{
    /// <inheritdoc />
    public partial class AlterOrdertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orders_addresses_billing_address_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "FK_orders_addresses_shipping_address_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_billing_address_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_shipping_address_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "billing_address_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipping_address_id",
                table: "orders");

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "orders",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ShippingPhone",
                table: "orders",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ShippingRecipientName",
                table: "orders",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ShippingPhone",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "ShippingRecipientName",
                table: "orders");

            migrationBuilder.AddColumn<Guid>(
                name: "billing_address_id",
                table: "orders",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "shipping_address_id",
                table: "orders",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_orders_billing_address_id",
                table: "orders",
                column: "billing_address_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_shipping_address_id",
                table: "orders",
                column: "shipping_address_id");

            migrationBuilder.AddForeignKey(
                name: "FK_orders_addresses_billing_address_id",
                table: "orders",
                column: "billing_address_id",
                principalTable: "addresses",
                principalColumn: "address_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_orders_addresses_shipping_address_id",
                table: "orders",
                column: "shipping_address_id",
                principalTable: "addresses",
                principalColumn: "address_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
