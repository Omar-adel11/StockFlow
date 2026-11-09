using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;
using Moq;
using static Application.DTOs.ProductDtos;

namespace StockFlow.tests.Services
{
    public class ProductServiceTests
    {

        [Fact]
        public async Task GetProductByIdAsync_CacheHit_ReturnsProductWithoutCallingInnerService()
        {
            //Arrange
            var mockCache = new Mock<ICacheService>();
            var mockProduct = new Mock<IProductService>();

            int productId = 1;
            var cachedProduct = new ProductResponse(productId, "TshtS", "Nike-Tshirt", 800, 500, 5, 1, "fashion", null, null, true);

            // Setup the cache to directly return the product (bypassing the factory/inner service)
            mockCache
                .Setup(c => c.GetOrCreateWithLockAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<ProductResponse?>>>(),
                    It.IsAny<TimeSpan>()))
                .ReturnsAsync(cachedProduct);

            var Service = new CachedProductService(mockProduct.Object,mockCache.Object);
            //Act
            var result = await Service.GetProductByIdAsync(productId);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(cachedProduct.Id, result.Id);

            mockProduct.Verify(s => s.GetProductByIdAsync(productId), Times.Never);


        }

        [Fact]
        public async Task GetProductByIdAsync_CacheMiss_ReturnsProductWithCallingInnerService()
        {
            //Arrange
            var mockCache = new Mock<ICacheService>();
            var mockProduct = new Mock<IProductService>();

            int productId = 1;
            var Product = new ProductResponse(productId, "TshtS", "Nike-Tshirt", 800, 500, 5, 1, "fashion", null, null, true);

            // Simulate a cache miss by executing the factory
            mockProduct.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync(Product);

            mockCache
                .Setup(c => c.GetOrCreateWithLockAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<ProductResponse?>>>(),
                    It.IsAny<TimeSpan>()))
                .Returns((
                        string key,
                        Func<Task<ProductResponse?>> factory,
                        TimeSpan duration) => factory());

            var Service = new CachedProductService(mockProduct.Object, mockCache.Object);
            //Act
            var result = await Service.GetProductByIdAsync(productId);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(Product.Id, result.Id);

            mockProduct.Verify(s => s.GetProductByIdAsync(productId), Times.Once);


        }


        [Fact]
        public async Task UpdateProductAsync_UpdatedProduct_InvalidatesRelatedCaches()
        {
            //arrange
            var mockProductService = new Mock<IProductService>();   
            var mockCacheService = new Mock<ICacheService>();

            int id = 1;
            int businessId = 1;
            var productUpdateRequest = new ProductUpdateRequest("T-Shirt-Nike", 800, 500, 5, 1, null, true);
            var productResponse = new ProductResponse(id, "TshtS", "T-Shirt-Nike", 800, 500, 5, 1, "fashion", null, null, true);


            mockProductService.Setup(r => r.UpdateProductAsync(id, It.IsAny<ProductUpdateRequest>(), businessId)).ReturnsAsync(productResponse);
            mockCacheService.Setup(c => c.RemoveAsync("products:id:1")).Returns(Task.CompletedTask);
            mockCacheService.Setup(c => c.RemoveAsync("products:sku:TshtS")).Returns(Task.CompletedTask);
            mockCacheService.Setup(c => c.RemoveAsync($"products:biz:{businessId}:list")).Returns(Task.CompletedTask);

            //act
            var cachedProductService = new CachedProductService(mockProductService.Object, mockCacheService.Object);
            var result = await cachedProductService.UpdateProductAsync(id,productUpdateRequest, businessId);

            //assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            mockProductService.Verify(p => p.UpdateProductAsync(id, It.IsAny<ProductUpdateRequest>(), businessId), Times.Once);
            mockCacheService.Verify(c => c.RemoveAsync("products:id:1"), Times.Once);
            mockCacheService.Verify(c => c.RemoveAsync("products:sku:TshtS"), Times.Once);
            mockCacheService.Verify(c => c.RemoveAsync($"products:biz:{businessId}:list"), Times.Once);
        }

        [Fact]
        public async Task CachedProductService_ProductNotFound_ReturnNotFoundException()
        {
            // Arrange
            var mockCacheService = new Mock<ICacheService>();
            var mockProductService = new Mock<IProductService>();

            int productId = 1;

            mockProductService.Setup(p => p.GetProductByIdAsync(productId)).ThrowsAsync( new ProductNotFoundException());

            mockCacheService.Setup(c => c.GetOrCreateWithLockAsync(It.IsAny<string>(),
            It.IsAny<Func<Task<ProductResponse?>>>(),
            It.IsAny<TimeSpan>())).Returns((string key,
            Func<Task<ProductResponse?>> factory,
            TimeSpan duration) => factory());

            var cachedProductService = new CachedProductService(mockProductService.Object, mockCacheService.Object);

            // Act & Assert
            
            var exception = await Assert.ThrowsAsync<ProductNotFoundException>(() => cachedProductService.GetProductByIdAsync(productId));

            Assert.Equal("Product Not Found", exception.Message);

        }

        [Fact]
        public async Task DeleteProductAsync_ProductInStock_ThrowInvalidOperationException()
        {

            // Arrange
            var mockCacheService = new Mock<ICacheService>();
            var mockProductService = new Mock<IProductService>();

            int productId = 1;
            int businessId = 1;

            var product = new ProductResponse(productId,"TshtS","T-Shirt-Nike",800, 500, 5, 1,"fashion",null, null, true);


            mockProductService.Setup(p => p.GetProductByIdAsync(productId)).ReturnsAsync(product);
            mockProductService.Setup(p => p.DeleteProductAsync(productId,businessId)).ThrowsAsync(new InvalidOperationException($"Cannot delete Product '{product.Name}' because it currently holds active stock."));
            var cachedProductService = new CachedProductService(mockProductService.Object, mockCacheService.Object);

            // act & assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => cachedProductService.DeleteProductAsync(productId,businessId));
            Assert.Equal($"Cannot delete Product '{product.Name}' because it currently holds active stock.", exception.Message);
           
            
            mockProductService.Verify(p => p.GetProductByIdAsync(productId),Times.Once);
            mockProductService.Verify(p => p.DeleteProductAsync(productId, businessId),Times.Once);
            mockCacheService.Verify(c => c.RemoveAsync("products:id:1"), Times.Never);
            mockCacheService.Verify(c => c.RemoveAsync("products:sku:TshtS"), Times.Never);
            mockCacheService.Verify(c => c.RemoveAsync($"products:biz:{businessId}:list"), Times.Never);
        }

      



        [Fact]
        public async Task GetProductByIdAsync_ValidProduct_ReturnsProduct()
        {
            // Arrange
            using var dbContext = new InMemoryDbContext();

            // Related entity must pass the global filter (BusinessId == 1 && IsActive)
            var category = new Domain.Entities.Category
            {
                Name = "Clothes",
                BusinessId = 1,
                IsActive = true
            };
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();

            var product = new Domain.Entities.Product
            {
                SKU = "TshtS",
                Name = "T-Shirt-Nike",
                UnitSellingPrice = 800,
                UnitPrice = 500,
                ReorderLevel = 5,
                CategoryId = category.Id,
                PreferredSupplierId = null, // no supplier needed for this test
                BusinessId = 1,             // must match CurrentBusinessId
                IsActive = true             // must be true for the global filter
            };
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();

            var service = new ProductService(dbContext);

            // Act
            var result = await service.GetProductByIdAsync(product.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(product.Id, result.Id);
            Assert.Equal("T-Shirt-Nike", result.Name);
            Assert.Equal("TshtS", result.ItemSKU);
            Assert.Equal("Clothes", result.CategoryName);
        }

    }
}
