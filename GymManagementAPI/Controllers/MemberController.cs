using GymManagementAPI.Data;
using GymManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GymManagementAPI.Controllers
{
    [Authorize(Roles = "Gymowner,Member")]
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly AppDbContext _context;
        public MemberController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public IActionResult AddMember(Member member)
        {
            _context.Members.Add(member);
            _context.SaveChanges();
            return Ok(member);
        }

        [AllowAnonymous]
        [HttpGet("gym/{gymId}")]
        public IActionResult GetMembersByGym(int gymId)
        {
            var members = (from m in _context.Members
                           join u in _context.Users on m.UserId equals u.Id
                           join t in _context.Trainers on m.TrainerId equals t.Id into trainerGroup
                           from t in trainerGroup.DefaultIfEmpty()
                           join p in _context.Plans on m.PlanId equals p.Id
                           where m.GymId == gymId
                           select new
                           {
                               m.Id,
                               m.MemberName,
                               m.Age,
                               m.Email,
                               m.GymId,
                               m.TrainerId,
                               TrainerName = t != null ? t.TrainerName : null,
                               m.UserId,
                               PhoneNumber = u.PhoneNumber,
                               m.PlanId,
                               PlanName = p.PlanName
                           }).ToList();

            return Ok(members);
        }

        [HttpPost]
        public IActionResult CreateMember(MemberCreateDto dto)
        {
            // Check if member already exists for this user+plan (change uniqueness rule if needed)
            var existingMember = _context.Members
                .FirstOrDefaultAsync(m => m.UserId == dto.UserId && m.PlanId == dto.PlanId);
            if (existingMember!=null)
            {
                return Ok("Member already exists");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == dto.UserId);
            if (user == null)
                return NotFound("User not found");

            int age = 0;
            if (user.DateOfBirth.HasValue)
            {
                var dob = user.DateOfBirth.Value;
                age = DateTime.Now.Year - dob.Year;
                if (dob > DateTime.Now.AddYears(-age)) age--;
            }
            var member = new Member
            {
                MemberName = user.FullName,
                Age = age,
                Email = user.Email,
                GymId = dto.GymId,
                TrainerId = dto.TrainerId,
                UserId = dto.UserId,
                PlanId = dto.PlanId
            };

            _context.Members.Add(member);
            _context.SaveChanges();

            return Ok("Member has been added sucessfully");
        }

        [HttpGet("isMember")]
        public IActionResult IsUserMember(int userId, int gymId)
        {
            var exists = _context.Members
                .Any(m => m.UserId == userId && m.GymId == gymId);

            return Ok(exists);
        }

    }
}
