using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessTableandteamInvitationtableformultitenancyaddindexesonalltablebyBusinessIdandcompositeonewithisActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Plans_PlanId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PlanId",
                table: "AspNetUsers",
                newName: "BusinessId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_PlanId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_BusinessId");

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Warehouses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Suppliers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "StockMovements",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "SalesOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "SalesOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "PurchaseOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "PurchaseOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "InventoryItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "CustomerAddresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessId",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Business",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Business", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Business_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeamInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BusinessId = table.Column<int>(type: "int", nullable: false),
                    InvitedByUserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamInvitations_AspNetUsers_InvitedByUserId",
                        column: x => x.InvitedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamInvitations_Business_BusinessId",
                        column: x => x.BusinessId,
                        principalTable: "Business",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_BusinessId_IsActive",
                table: "Warehouses",
                columns: new[] { "BusinessId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_BusinessId_IsActive",
                table: "Suppliers",
                columns: new[] { "BusinessId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_BusinessId",
                table: "StockMovements",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrders_BusinessId",
                table: "SalesOrders",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderItems_BusinessId",
                table: "SalesOrderItems",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_BusinessId",
                table: "PurchaseOrders",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_BusinessId",
                table: "PurchaseOrderItems",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BusinessId_IsActive",
                table: "Products",
                columns: new[] { "BusinessId", "IsActive" },
                unique: true,
                filter: "[BusinessId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BusinessId_SKU",
                table: "Products",
                columns: new[] { "BusinessId", "SKU" },
                unique: true,
                filter: "[BusinessId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_BusinessId",
                table: "InventoryItems",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_BusinessId",
                table: "Customers",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddresses_BusinessId",
                table: "CustomerAddresses",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_BusinessId_IsActive",
                table: "Categories",
                columns: new[] { "BusinessId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Business_PlanId",
                table: "Business",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_BusinessId",
                table: "TeamInvitations",
                column: "BusinessId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_InvitedByUserId",
                table: "TeamInvitations",
                column: "InvitedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamInvitations_Token",
                table: "TeamInvitations",
                column: "Token",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Business_BusinessId",
                table: "AspNetUsers",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Business_BusinessId",
                table: "Categories",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerAddresses_Business_BusinessId",
                table: "CustomerAddresses",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Business_BusinessId",
                table: "Customers",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Business_BusinessId",
                table: "InventoryItems",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Business_BusinessId",
                table: "Products",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_Business_BusinessId",
                table: "PurchaseOrderItems",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Business_BusinessId",
                table: "PurchaseOrders",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderItems_Business_BusinessId",
                table: "SalesOrderItems",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrders_Business_BusinessId",
                table: "SalesOrders",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Business_BusinessId",
                table: "StockMovements",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Suppliers_Business_BusinessId",
                table: "Suppliers",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Business_BusinessId",
                table: "Warehouses",
                column: "BusinessId",
                principalTable: "Business",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Business_BusinessId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Business_BusinessId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerAddresses_Business_BusinessId",
                table: "CustomerAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Business_BusinessId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Business_BusinessId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Business_BusinessId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_Business_BusinessId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Business_BusinessId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderItems_Business_BusinessId",
                table: "SalesOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrders_Business_BusinessId",
                table: "SalesOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Business_BusinessId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_Suppliers_Business_BusinessId",
                table: "Suppliers");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Business_BusinessId",
                table: "Warehouses");

            migrationBuilder.DropTable(
                name: "TeamInvitations");

            migrationBuilder.DropTable(
                name: "Business");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_BusinessId_IsActive",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_BusinessId_IsActive",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_BusinessId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrders_BusinessId",
                table: "SalesOrders");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderItems_BusinessId",
                table: "SalesOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_BusinessId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_BusinessId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Products_BusinessId_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_BusinessId_SKU",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_BusinessId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_Customers_BusinessId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerAddresses_BusinessId",
                table: "CustomerAddresses");

            migrationBuilder.DropIndex(
                name: "IX_Categories_BusinessId_IsActive",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "SalesOrderItems");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "BusinessId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "BusinessId",
                table: "AspNetUsers",
                newName: "PlanId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_BusinessId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Plans_PlanId",
                table: "AspNetUsers",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id");
        }
    }
}
