using GymManagementAPI.Data;
using GymManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GymManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainerController : ControllerBase
    {
        private readonly AppDbContext _context;
        public TrainerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add/{gymId}")]
        public IActionResult AddTrainer([FromBody] Trainer trainer, int gymId)
        {
            trainer.GymId = gymId; // Explicitly assign the gymId
            _context.Trainers.Add(trainer);
            _context.SaveChanges();
            return Ok(trainer);
        }


        [HttpGet("all/{gymId}")]
        public IActionResult GetTrainersByGymId(int gymId)
        {
            var trainers = _context.Trainers.Where(t => t.GymId == gymId).ToList();
            return Ok(trainers);
        }


        [HttpDelete("deleteByGymId/{gymId}")]
        public async Task<IActionResult> DeleteTrainersByGymId(int gymId)
        {
            var sql = "DELETE FROM Trainers WHERE GymId = @gymId";

            try
            {
                var parameters = new[] { new SqlParameter("@gymId", gymId) };
                await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                return Ok("✅ Trainers deleted for Gym ID: " + gymId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "❌ Error deleting trainers: " + ex.Message);
            }
        }
    }
}
