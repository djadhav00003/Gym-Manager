using GymManagementAPI.Data;
using GymManagementAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GymManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
                return BadRequest("Email already exists.");

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(user);
        }

        [HttpPost("login")]
        public IActionResult Login(User loginUser)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Email == loginUser.Email &&
                u.Password == loginUser.Password &&
                u.Role == loginUser.Role);

            if (user == null)
                return Unauthorized("Invalid email, password, or role.");

            // Prepare the response
            var userResponse = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Password = user.Password,
                Role = user.Role,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address
            };

            // Check if the user is a member
            if (loginUser.Role == "Member")
            {
                userResponse.isMember = _context.Members.Any(m => m.UserId == user.Id);
            }

            return Ok(userResponse);
        }


        [HttpGet("getUserByEmail/{email}")]
        public IActionResult GetUserByEmail(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }


        // CHECK EMAIL EXISTS (EXCLUDING CURRENT USER)
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmailExists([FromQuery] string email)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == email);
            return Ok(exists);
        }

        // UPDATE PROFILE
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] User updatedUser)
        {
            var existingUser = await _context.Users.FindAsync(updatedUser.Id);
            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            // Optional: Check if email is changed and unique
            if (existingUser.Email != updatedUser.Email)
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == updatedUser.Email && u.Id != updatedUser.Id);
                if (emailExists)
                {
                    return Conflict("Email already in use.");
                }
            }

            // Update fields
            existingUser.FullName = updatedUser.FullName;
            existingUser.Email = updatedUser.Email;

            await _context.SaveChangesAsync();

            return Ok(existingUser);
        }
    }
}
