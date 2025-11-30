using GymManagementAPI.Data;
using GymManagementAPI.Models;
using GymManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GymManagementAPI.Controllers
{
    [Authorize(Roles = "Admin,Gymowner")]
    [ApiController]
    [Route("api/[controller]")]
    public class GymController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FirebaseStorageService _firebaseStorageService;
        public GymController(AppDbContext context, FirebaseStorageService firebaseStorageService)
        {
            _context = context;
            _firebaseStorageService = firebaseStorageService;

        }
       
        [HttpPost("add/{userId}")]
        public IActionResult AddGym(Gym gym,int userId)
        {
            gym.OwnerUserId = userId;
            _context.Gyms.Add(gym);
            _context.SaveChanges();
            return Ok(gym);
        }
        [AllowAnonymous]
        [HttpGet("all")]
        public IActionResult GetAllGyms()
        {
            var gymsWithTrainerCount = _context.Gyms
                .Where(g=> !g.IsDeleted)
                .Select(g => new
                {
                    g.Id,
                    g.GymName,
                    g.Location,
                    g.Contact,
                    g.Facilities,
                    TrainerCount = g.Trainers.Count() // Navigation property
                })
                .AsNoTracking()
                .ToList();

            return Ok(gymsWithTrainerCount);
        }

        [HttpGet("owner/{userId}")]
        public IActionResult GetGymsForOwner(int userId)
        {
            var ownerGyms = _context.Gyms
                .Where(g => g.OwnerUserId == userId && !g.IsDeleted)   // filter by owner + not deleted
                .Select(g => new
                {
                    g.Id,
                    g.GymName,
                    g.Location,
                    g.Contact,
                    g.Facilities,
                    TrainerCount = g.Trainers.Count()
                })
                .AsNoTracking()
                .ToList();

            return Ok(ownerGyms);
        }

        [AllowAnonymous]
        [HttpGet("all-except-user/{userId}")]
        public IActionResult GetGymsExceptUser(int userId)
        {
            // Step 1: Get all gymIds the user is already a member of
            var userGymIds = _context.Members
                .Where(m => m.UserId == userId)
                .Select(m => m.GymId)
                .ToList();

            // Step 2: Get all gyms except these
            var gyms = _context.Gyms
                .Where(g => !userGymIds.Contains(g.Id) && !g.IsDeleted)
                .Select(g => new
                {
                    g.Id,
                    g.GymName,
                    g.Location,
                    g.Contact,
                    g.Facilities,
                    TrainerCount = g.Trainers.Count()
                })
                .AsNoTracking()
                .ToList();

            return Ok(gyms);
        }


        [AllowAnonymous]
        [HttpGet("withMembership/{userId}")]
        public IActionResult GetGymsWithMembership(int userId)
        {
            var gymsWithMembership = _context.Members
                .Where(m => m.UserId == userId)
                .Select(m => m.Gym)
                .Distinct()
                .Select(g => new
                {
                    g.Id,
                    g.GymName,
                    g.Location,
                    g.Contact,
                    g.Facilities,
                    TrainerCount = g.Trainers.Count()
                })
                .AsNoTracking()
                .ToList();

            return Ok(gymsWithMembership);
        }


        [HttpPut("update")]
        public IActionResult UpdateGym([FromBody] Gym updatedGym)
        {
            var gym = _context.Gyms.Find(updatedGym.Id);
            if (gym == null) return NotFound("Gym not found");

            gym.GymName = updatedGym.GymName;
            gym.Location = updatedGym.Location;
            gym.Contact = updatedGym.Contact;
            gym.Facilities = updatedGym.Facilities;

            _context.SaveChanges();
            return Ok(new { message = "Gym updated" });
        }


        [HttpDelete("delete/{gymId}/{userId}")]
        public async Task<IActionResult> DeleteGymAndTrainers(int gymId, int userId)
        {
            if (gymId <= 0) return BadRequest("Invalid gym id.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Load the gym (no related collections required for soft-delete)
                var gym = await _context.Gyms.FirstOrDefaultAsync(g => g.Id == gymId);
                if (gym == null)
                    return NotFound("Gym not found.");

                // Idempotent: if already deleted, return OK (or you can return Conflict)
                if (gym.IsDeleted)
                {
                    await transaction.CommitAsync();
                    return Ok("Gym is already deleted.");
                }

                // Soft-delete: mark flags and metadata
                gym.IsDeleted = true;
                gym.DeletedAt = DateTime.UtcNow;
                gym.DeletedByUserId = userId; // assume caller validated this user is an admin

                _context.Gyms.Update(gym);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok("Gym marked as deleted.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log exception via ILogger in real code
                return StatusCode(500, "Error marking gym as deleted: " + ex.Message);
            }
        }

        [HttpPost("{gymId}/images")]
        public async Task<IActionResult> UploadMultipleGymImages(int gymId, [FromBody] GymImagesDto dto)
        {
            var gym = await _context.Gyms.FindAsync(gymId);
            if (gym == null)
                return NotFound(new { message = "Gym not found" });

            var gymImages = dto.ImageUrls.Select(url => new GymImages
            {
                GymId = gymId,
                ImageUrl = url,
                UploadedAt = DateTime.Now
            }).ToList();

            _context.GymImages.AddRange(gymImages);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Images uploaded successfully", count = gymImages.Count });
        }
        [AllowAnonymous]
        [HttpGet("images/{gymId}")]
        public async Task<IActionResult> GetGymImages(int gymId)
        {
            var images = await _context.GymImages
                .Where(i => i.GymId == gymId)
                .Select(i => i.ImageUrl)
                .ToListAsync();

            return Ok(images);
        }
    }
}
