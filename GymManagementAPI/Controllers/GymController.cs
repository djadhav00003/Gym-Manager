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
    public class GymController : ControllerBase
    {
        private readonly AppDbContext _context;
        public GymController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public IActionResult AddGym(Gym gym)
        {
            _context.Gyms.Add(gym);
            _context.SaveChanges();
            return Ok(gym);
        }

        [HttpGet("all")]
        public IActionResult GetAllGyms()
        {
            var gymsWithTrainerCount = _context.GymTrainerCounts
    .FromSqlRaw(@"
        SELECT 
            g.Id,
            g.GymName,
            g.Location,
            g.Contact,
            g.Facilities,
            COUNT(t.Id) AS TrainerCount
        FROM Gyms g
        LEFT JOIN Trainers t ON g.Id = t.GymId
        GROUP BY g.Id, g.GymName, g.Location, g.Contact, g.Facilities
    ")
    .AsNoTracking()
    .ToList();

            return Ok(gymsWithTrainerCount);
        }

        [HttpGet("withMembership/{userId}")]
        public IActionResult GetGymsWithMembership(int userId)
        {
            var gymsWithTrainerCount = _context.GymTrainerCounts
                .FromSqlRaw(@"
            SELECT DISTINCT g.Id, g.GymName, g.Location, g.Contact, g.Facilities,
                   (SELECT COUNT(*) FROM Trainers t WHERE t.GymId = g.Id) AS TrainerCount
            FROM Gyms g
            INNER JOIN Members m ON g.Id = m.GymId
            WHERE m.UserId = {0}
        ", userId)
                .AsNoTracking()
                .ToList();

            return Ok(gymsWithTrainerCount);
        }

        [HttpPut("update")]
        public IActionResult UpdateGym([FromBody] Gym updatedGym)
        {
            var sql = @"
        UPDATE Gyms
        SET GymName = @GymName,
            Location = @Location,
            Contact = @Contact,
            Facilities = @Facilities
        WHERE Id = @Id";

            var result = _context.Database.ExecuteSqlRaw(sql,
                new[]
                {
            new SqlParameter("@Id", updatedGym.Id),
            new SqlParameter("@GymName", updatedGym.GymName),
            new SqlParameter("@Location", updatedGym.Location),
            new SqlParameter("@Contact", updatedGym.Contact ?? (object)DBNull.Value),
            new SqlParameter("@Facilities", updatedGym.Facilities ?? (object)DBNull.Value)
                });

            return Ok(new { message = "Gym updated", rowsAffected = result });
        }

  
        [HttpDelete("delete/{gymId}")]
        public async Task<IActionResult> DeleteGymAndTrainers(int gymId)
        {
            var deleteTrainersSql = "DELETE FROM Trainers WHERE GymId = @gymId";
            var deleteGymSql = "DELETE FROM Gyms WHERE Id = @gymId";

            var param = new SqlParameter("@gymId", gymId);

            try
            {
                // Delete trainers first
                await _context.Database.ExecuteSqlRawAsync(deleteTrainersSql, param);

                // Now delete the gym
                await _context.Database.ExecuteSqlRawAsync(deleteGymSql, param);

                return Ok("✅ Gym and its trainers deleted successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "❌ Error deleting gym or trainers: " + ex.Message);
            }
        }
    }
}
