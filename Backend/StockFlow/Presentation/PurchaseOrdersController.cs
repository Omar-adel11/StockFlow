using System.Security.Claims;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.PurchaseOrderDtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PurchaseOrdersController(IServiceManager serviceManager) : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int count = 50)
        {
            var orders = await _serviceManager.PurchaseOrderService.GetAllOrdersAsync(count);
            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _serviceManager.PurchaseOrderService.GetOrderByIdAsync(id);
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PurchaseCreateRequest createRequest)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var createdOrder = await _serviceManager.PurchaseOrderService.CreatePurchaseOrderAsync(createRequest, userId);
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpPut("{id:int}/receive")]
        public async Task<IActionResult> Receive(int id)
        {
            var result = await _serviceManager.PurchaseOrderService.ReceivePurchaseOrderAsync(id);
            return result ? Ok(new { Message = "Purchase order received and inventory updated." }) : BadRequest("Order cannot be received.");
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _serviceManager.PurchaseOrderService.CancelPurchaseOrderAsync(id);
            return result ? Ok(new { Message = "Purchase order cancelled." }) : BadRequest("Order cannot be cancelled.");
        }
    }
}