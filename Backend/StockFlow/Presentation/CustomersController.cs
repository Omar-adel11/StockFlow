using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Application.Services.Helper;
using Domain.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Attributes;
using static Application.DTOs.CustomerDtos;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager},{Roles.Staff}")]
    [RequireTenant]
    public class CustomersController(IServiceManager serviceManager) : ControllerBase
    {
        private readonly IServiceManager _serviceManager = serviceManager;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            int businessId = User.GetBusinessId();
            var customers = await _serviceManager.CustomerService.GetAllCustomersAsync(search, businessId);
            return Ok(customers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _serviceManager.CustomerService.GetCustomerAsync(id);
            return customer == null ? NotFound() : Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerCreateRequest dto)
        {
            var businessId = User.GetBusinessId();

            var createdCustomer = await _serviceManager.CustomerService.CreateCustomerAsync(dto,businessId);
            return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, createdCustomer);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerUpdateRequest dto)
        {
            int businessId = User.GetBusinessId();
            var result = await _serviceManager.CustomerService.UpdateCustomerAsync(id, dto,businessId);
            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = $"{Roles.BusinessOwner},{Roles.Manager}")]
        public async Task<IActionResult> Delete(int id)
        {
            int businessId = User.GetBusinessId();
            var result = await _serviceManager.CustomerService.DeleteCustomerAsync(id,businessId);
            return result ? NoContent() : NotFound(new {message = $"customer with id {id} is not deleted"});
        }

        [HttpPost("{customerId:int}/addresses")]
        public async Task<IActionResult> AddAddress(int customerId, [FromBody] CustomerDtos.AddressSaveRequest request)
        {
            int businessId = User.GetBusinessId();
            var updatedCustomer = await _serviceManager.CustomerService.AddAddressAsync(customerId, request, businessId);
            return Ok(updatedCustomer);
        }

        // PUT api/customers/{customerId}/addresses/{addressId}
        [HttpPut("{customerId:int}/addresses/{addressId:int}")]
        public async Task<IActionResult> UpdateAddress(int customerId, int addressId, [FromBody] CustomerDtos.AddressSaveRequest request)
        {
            int businessId = User.GetBusinessId();
            await _serviceManager.CustomerService.UpdateAddressAsync(customerId, addressId, request, businessId);
            return NoContent();
        }

        // DELETE api/customers/{customerId}/addresses/{addressId}
        [HttpDelete("{customerId:int}/addresses/{addressId:int}")]
        public async Task<IActionResult> DeleteAddress(int customerId, int addressId)
        {
            int businessId = User.GetBusinessId();
            await _serviceManager.CustomerService.DeleteAddressAsync(customerId, addressId, businessId);
            return NoContent();
        }
    }
}