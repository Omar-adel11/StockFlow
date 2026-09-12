using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Presentation
{
    [ApiController]
    [Route("api/[controller]")]
    
        public class PlansController : ControllerBase
        {
            private readonly IPlanService _planService;

            public PlansController(IPlanService planService)
            {
                _planService = planService;
            }

            [HttpGet]
            public async Task<ActionResult<List<PlanResponseDto>>> GetAll()
            {
                var plans = await _planService.GetAllPlansAsync();
                return Ok(plans);
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<PlanResponseDto>> GetById(int id)
            {
                var plan = await _planService.GetPlanByIdAsync(id);
                if (plan is null)
                {
                    return NotFound(new { message = $"Plan with id {id} was not found." });
                }
                return Ok(plan);
            }

            [HttpPost]
            public async Task<ActionResult<PlanResponseDto>> Create([FromBody] PlanRequestDto request)
            {
                var created = await _planService.CreatePlanAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }

            [HttpPut("{id}")]
            public async Task<ActionResult<PlanResponseDto>> Update(int id, [FromBody] PlanRequestDto request)
            {
                var updated = await _planService.UpdatePlanAsync(id, request);
                if (updated is null)
                {
                    return NotFound(new { message = $"Plan with id {id} was not found." });
                }
                return Ok(updated);
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(int id)
            {
                var deleted = await _planService.DeletePlanAsync(id);
                if (!deleted)
                {
                    return NotFound(new { message = $"Plan with id {id} was not found." });
                }
                return NoContent();
            }
        }
    

}
