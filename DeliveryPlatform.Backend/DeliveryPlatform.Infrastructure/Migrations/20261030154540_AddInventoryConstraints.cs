using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventory_Quantity",
                table: "Inventories",
                sql: "[Quantity] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Inventory_ReservedQuantity",
                table: "Inventories",
                sql: "[ReservedQuantity] >= 0 AND [ReservedQuantity] <= [Quantity]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventory_Quantity",
                table: "Inventories");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Inventory_ReservedQuantity",
                table: "Inventories");
        }
    }
}
