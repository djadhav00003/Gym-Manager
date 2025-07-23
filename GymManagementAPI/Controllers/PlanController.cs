using GymManagementAPI.Data;
using GymManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GymManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PaymentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("pay")]
        public IActionResult MakePayment(Payment payment)
        {
            payment.PaymentDate = DateTime.Now;
            _context.Payments.Add(payment);
            _context.SaveChanges();
            return Ok(payment);
        }

        [HttpGet("gym/{gymId}")]
        public IActionResult GetPlansByGymId(int gymId)
        {
            var plans = _context.Plans
                .FromSqlRaw("SELECT * FROM Plans WHERE GymId = {0}", gymId)
                .ToList();

            return Ok(plans);
        }


        [HttpPost("add")]
        public async Task<IActionResult> AddPlan([FromBody] Plan plan)
        {
            if (plan == null)
            {
                return BadRequest("Invalid plan data.");
            }

            // Optional: Check if the Gym exists
            var gymExists = await _context.Gyms.AnyAsync(g => g.Id == plan.GymId);
            if (!gymExists)
            {
                return NotFound("Gym not found.");
            }

            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Plan added successfully", plan });
        }
    }
}
