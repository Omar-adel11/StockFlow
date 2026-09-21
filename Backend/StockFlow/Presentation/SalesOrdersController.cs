using System.Security.Claims;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.SalesOrderDtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesOrdersController(IServiceManager serviceManager) : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _serviceManager.SalesOrderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _serviceManager.SalesOrderService.GetOrderByIdAsync(id);
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SalesCreateRequest createRequest)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var createdOrder = await _serviceManager.SalesOrderService.CreateSalesOrderAsync(createRequest, userId);
            return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, createdOrder);
        }

        [HttpPut("{id:int}/fulfill")]
        public async Task<IActionResult> Fulfill(int id)
        {
            var result = await _serviceManager.SalesOrderService.FulfillSalesOrderAsync(id);
            return Ok(new { Message = "Sales order fulfilled and inventory deducted." });
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _serviceManager.SalesOrderService.CancelSalesOrderAsync(id);
            return result ? Ok(new { Message = "Sales order cancelled." }) : BadRequest("Order cannot be cancelled.");
        }
    }
}