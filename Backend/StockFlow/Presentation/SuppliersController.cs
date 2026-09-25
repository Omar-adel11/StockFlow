using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services.Helper;
using Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Attributes;
using static Application.DTOs.SuppliersDtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]
    [RequireTenant]
    public class SuppliersController(IServiceManager serviceManager) : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var suppliers = await _serviceManager.SupplierService.GetAllSuppliersAsync();
            return Ok(suppliers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var supplier = await _serviceManager.SupplierService.GetSupplierAsync(id);
            return Ok(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SupplierCreateRequest dto)
        {
            var businessId = User.GetBusinessId();
           
            var createdSupplier = await _serviceManager.SupplierService.CreateSupplierAsync(dto,businessId);
            return CreatedAtAction(nameof(GetById), new { id = createdSupplier.Id }, createdSupplier);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SupplierUpdateRequest dto)
        {
            var result = await _serviceManager.SupplierService.UpdateSupplierAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _serviceManager.SupplierService.DeleteSupplierAsync(id);
            return result ? NoContent() : NotFound(new {message = $"Supplier with id {id} is not deleted."});
        }
    }
}

    
