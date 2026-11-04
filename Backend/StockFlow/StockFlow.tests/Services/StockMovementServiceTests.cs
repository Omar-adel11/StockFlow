using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;
using Moq;
using static Application.DTOs.StockMovementDtos;

namespace StockFlow.tests.Services
{
    public class StockMovementServiceTests
    {
        [Theory]
        [InlineData(1, 2, 1, 10)]
        [InlineData(1, 2, 1, -2)]
        [InlineData(1, 2, 1, 0)]
        public async Task TransferProductAsync_InValidTransfer_throwInvalidOperationException(int fromId, int toId, int productId, int quantity)
        {
            // Arrange
            using var dbContext = new InMemoryDbContext();

            // Seed Product
            var product = new Product
            {
                Id = productId,
                Name = "Nike T-Shirt",
                SKU = "TSHT-01",
                UnitPrice = 500,
                UnitSellingPrice = 800,
                ReorderLevel = 5,
                BusinessId = 1,
                IsActive = true
            };
            await dbContext.Products.AddAsync(product);

            // Seed Warehouses
            var fromWarehouse = new Warehouse { Id = fromId, Name = "Warehouse A", BusinessId = 1, IsActive = true };
            var toWarehouse = new Warehouse { Id = toId, Name = "Warehouse B", BusinessId = 1, IsActive = true };
            await dbContext.Warehouses.AddRangeAsync(fromWarehouse, toWarehouse);

            // Seed Inventory Item with limited stock (e.g., 2 units)
            var inventoryItem = new InventoryItem
            {
                ProductId = productId,
                WarehouseId = fromId,
                QuantityOnHand = 2,
                BusinessId = 1
            };
            await dbContext.InventoryItems.AddAsync(inventoryItem);
            await dbContext.SaveChangesAsync();

          

            // Mock remaining service dependencies
            var mockCurrentUserService = new Mock<ICurrentUserService>();
            mockCurrentUserService.Setup(s => s.UserId).Returns("user-1");
            mockCurrentUserService.Setup(s => s.UserName).Returns("Test User");

            var mockEmailService = new Mock<IEmailService>();

            var service = new StockMovementService(dbContext, mockCurrentUserService.Object, mockEmailService.Object);

            var request = new WarehouseDtos.WarehouseProductExchangeRequest( fromId, toId, productId, quantity);
            var sourceInventory = await dbContext.InventoryItems.FirstAsync(i => i.ProductId == productId && i.WarehouseId == fromId);

            

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.TransferProductAsync(request, businessId: 1)
            );

            Assert.Equal(2, sourceInventory.QuantityOnHand);
        }
    }
    
}

