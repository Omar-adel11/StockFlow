using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addunitSellingPricecoltoproducttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "SalesOrderItems",
                newName: "UnitSellingPrice");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitSellingPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitSellingPrice",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "UnitSellingPrice",
                table: "SalesOrderItems",
                newName: "UnitPrice");
        }
    }
}
