using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Attributes;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager},{Roles.Staff}")]
    [RequireTenant]
    public class InventoryController(IServiceManager serviceManager) : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        [HttpGet]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _serviceManager.InventoryService.GetInventoryOverviewAsync();
            return Ok(overview);
        }

        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var inventory = await _serviceManager.InventoryService.GetInventoryByProductAsync(productId);
            return Ok(inventory);
        }

        [HttpGet("warehouse/{warehouseId:int}")]
        public async Task<IActionResult> GetByWarehouse(int warehouseId)
        {
            var inventory = await _serviceManager.InventoryService.GetInventoryByWarehouseAsync(warehouseId);
            return Ok(inventory);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockItems()
        {
            var lowStockItems = await _serviceManager.InventoryService.GetLowStockItemsAsync();
            return Ok(lowStockItems);
        }
    }
}