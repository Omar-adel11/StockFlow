using System.Runtime.InteropServices;
using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace StockFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        
        [HttpPost]
        public async Task<IActionResult> SubmitInquiry([FromBody] ContactRequestDto request)
        {
            var result = await _contactService.SubmitInquiryAsync(request);
            return Ok(result);
        }
    }
}
