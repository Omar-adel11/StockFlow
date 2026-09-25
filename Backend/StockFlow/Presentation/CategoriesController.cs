using System.Threading.Tasks;
using Application.Interfaces;
using Application.Services.Helper;
using Domain.Entities;
using Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Attributes;
using static Application.DTOs.CategoryDtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]
    [RequireTenant]
    public class CategoriesController(IServiceManager serviceManager) : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        [HttpGet]
        [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager},{Roles.Staff}")]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _serviceManager.CategoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _serviceManager.CategoryService.GetCategoryAsync(id);
            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRequest dto)
        {
            var businessId = User.GetBusinessId();
            var createdCategory = await _serviceManager.CategoryService.CreateCategoryAsync(dto, businessId);
            return CreatedAtAction(nameof(GetById), new { id = createdCategory.Id }, createdCategory);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRequest dto)
        {
            var result = await _serviceManager.CategoryService.UpdateCategoryAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _serviceManager.CategoryService.DeleteCategoryAsync(id);
            return result ? NoContent() : NotFound(new { Message = $"Category with ID {id} is not deleted." });
        }
    }
}