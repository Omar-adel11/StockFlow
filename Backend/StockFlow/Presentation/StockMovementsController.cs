using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.StockMovementDtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StockMovementsController(IServiceManager serviceManager) : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 20)
        {
            var movements = await _serviceManager.StockMovementService.GetRecentMovementsAsync(count);
            return Ok(movements);
        }

        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProduct(int productId)
        {
            var movements = await _serviceManager.StockMovementService.GetMovementsByProductAsync(productId);
            return Ok(movements);
        }

        [HttpGet("warehouse/{warehouseId:int}")]
        public async Task<IActionResult> GetByWarehouse(int warehouseId)
        {
            var movements = await _serviceManager.StockMovementService.GetMovementsByWarehouseAsync(warehouseId);
            return Ok(movements);
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> CreateManualAdjustment([FromBody] ManualAdjustmentRequest request)
        {
            var result = await _serviceManager.StockMovementService.CreateManualAdjustmentAsync(request);
            return result
                ? Ok(new { Message = "Stock adjustment executed successfully." })
                : BadRequest(new { Message = "Failed to record stock adjustment." });
        }
    }
}