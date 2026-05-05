using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ims_inv.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUnitConversionProductWise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitConversion_FromTo",
                table: "UnitConversions");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "UnitConversions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UnitConversion_Product_FromTo",
                table: "UnitConversions",
                columns: new[] { "ProductId", "FromUnitId", "ToUnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitConversions_FromUnitId",
                table: "UnitConversions",
                column: "FromUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_UnitConversions_Products_ProductId",
                table: "UnitConversions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitConversions_Products_ProductId",
                table: "UnitConversions");

            migrationBuilder.DropIndex(
                name: "IX_UnitConversion_Product_FromTo",
                table: "UnitConversions");

            migrationBuilder.DropIndex(
                name: "IX_UnitConversions_FromUnitId",
                table: "UnitConversions");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "UnitConversions");

            migrationBuilder.CreateIndex(
                name: "IX_UnitConversion_FromTo",
                table: "UnitConversions",
                columns: new[] { "FromUnitId", "ToUnitId" },
                unique: true);
        }
    }
}
