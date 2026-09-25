using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services.Helper;
using Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Attributes;
using static Application.DTOs.ProductDtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]
    [RequireTenant]
    public class ProductsController(IServiceManager serviceManager) : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        [HttpGet]
        [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager},{Roles.Staff}")]
        public async Task<IActionResult> GetAll()
        {
            var products = await _serviceManager.ProductService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _serviceManager.ProductService.GetProductByIdAsync(id);
            return Ok(product);
        }

        [HttpGet("sku/{sku}")]
        public async Task<IActionResult> GetBySku(string sku)
        {
            var product = await _serviceManager.ProductService.GetProductBySkuAsync(sku);
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateRequest createRequest)
        {
            var businessId = User.GetBusinessId();
            
            var createdProduct = await _serviceManager.ProductService.CreateProductAsync(createRequest,businessId);
            return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, createdProduct);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateRequest updateRequest)
        {
            var updatedProduct = await _serviceManager.ProductService.UpdateProductAsync(id, updateRequest);
            return Ok(updatedProduct);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _serviceManager.ProductService.DeleteProductAsync(id);
            return result ? NoContent() : NotFound(new { Message = $"Product with ID {id} was not found." });
        }
    }
}