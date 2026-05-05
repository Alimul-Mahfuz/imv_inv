using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ims_inv.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWarehouseIdFromInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_Warehouses_WarehouseId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_WarehouseId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Inventory_Product_Warehouse",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ProductId",
                table: "Inventories",
                column: "ProductId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inventory_ProductId",
                table: "Inventories");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Inventories",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_WarehouseId",
                table: "Inventories",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Product_Warehouse",
                table: "Inventories",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_Warehouses_WarehouseId",
                table: "Inventories",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
