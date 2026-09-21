using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Application.DTOs.WarehouseDtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WarehousesController(IServiceManager serviceManager) : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var warehouses = await _serviceManager.WarehouseService.GetAllWarehousesAsync();
            return Ok(warehouses);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var warehouse = await _serviceManager.WarehouseService.GetWarehouseAsync(id);
            return Ok(warehouse);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WarehouseCreateRequest dto)
        {
            var createdWarehouse = await _serviceManager.WarehouseService.CreateWarehouseAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdWarehouse.Id }, createdWarehouse);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] WarehouseUpdateRequest dto)
        {
            var result = await _serviceManager.WarehouseService.UpdateWarehouseAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _serviceManager.WarehouseService.DeleteWarehouseAsync(id);
            return result ? NoContent() : NotFound(new { mesage = $"warehouse with ID {id} is not deleted" });
        }
    }
}