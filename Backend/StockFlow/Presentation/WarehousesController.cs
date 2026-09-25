using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services.Helper;
using Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Attributes;
using static Application.DTOs.WarehouseDtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]
    [RequireTenant]
    public class WarehousesController(IServiceManager serviceManager) : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        [HttpGet]
        [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager},{Roles.Staff}")]
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
            var businessId = User.GetBusinessId();
           
            var createdWarehouse = await _serviceManager.WarehouseService.CreateWarehouseAsync(dto,businessId);
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