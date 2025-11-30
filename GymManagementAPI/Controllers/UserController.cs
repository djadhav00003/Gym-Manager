using GymManagementAPI.Data;
using GymManagementAPI.Models;
using GymManagementAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GymManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<UserController> _logger;
        public UserController(AppDbContext context, JwtService jwtService, IWebHostEnvironment env,
                          ILogger<UserController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _env = env;
            _logger = logger;
        }
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]User user)
        {
            if (user == null) return BadRequest("Invalid payload.");
            if (string.IsNullOrWhiteSpace(user.Email)) return BadRequest("Email required.");
            if (string.IsNullOrWhiteSpace(user.Password)) return BadRequest("Password required.");

            // avoid race: check async and rely on unique index in DB (add migration if not present)
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest("Email already exists.");

            // hash the password before saving
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role = "Member";
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // remove sensitive fields before returning
            var safe = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role
            };

            return Ok(safe);
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (model == null) return BadRequest("Invalid payload.");
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Email and password required.");

            var user = await _context.Users
       .Include(u => u.RefreshTokens) // include tokens to check existing ones
       .FirstOrDefaultAsync(u => u.Email == model.Email  && u.Role == model.Role);

            if (user == null )
                return Unauthorized("Invalid credentials");

            // Verify hashed password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);

              if (!isPasswordValid)
                return Unauthorized("Invalid credentials");
           
                var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenHash = ComputeSha256Hash(refreshToken);
            var jwtId = Guid.NewGuid().ToString();


            // ✅ Remove expired or revoked tokens only (keep valid ones)
            var expiredTokens = user.RefreshTokens
                .Where(t => t.ExpiresAt <= DateTime.UtcNow || t.IsRevoked)
                .ToList();
            if (expiredTokens.Any())
                _context.UserRefreshTokens.RemoveRange(expiredTokens);

            // Save refresh token to DB
            user.RefreshTokens.Add(new UserRefreshToken
            {
                Token = refreshTokenHash,
                JwtId = jwtId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id
            });
            await _context.SaveChangesAsync();

            //// Set tokens in HTTP-only cookies
            //Response.Cookies.Append("AccessToken", accessToken, new CookieOptions
            //{
            //    HttpOnly = true,
            //    Secure = true,
            //    SameSite = SameSiteMode.None,      // ✅ Required for cross-site requests
            //    Path = "/",
            //    Expires = DateTime.UtcNow.AddMinutes(15)
            //});

            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,      // ✅ Required for cross-site requests
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                message = "Login successful",
                accessToken = accessToken,
                email = user.Email,
                id = user.Id,
                isMember = user.Role == "Member",
                isAdmin = user.Role == "Admin",
                role = user.Role
            });
        }

        [AllowAnonymous] // cookie-based refresh, no JWT present
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            // read raw refresh token from cookie
            var refreshToken = Request.Cookies["RefreshToken"];
            _logger.LogInformation("Refresh-token endpoint hit. Cookie present? {HasCookie}", !string.IsNullOrEmpty(refreshToken));
            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("RefreshToken cookie missing on request from {RemoteIp}", HttpContext.Connection.RemoteIpAddress);
                return Unauthorized("Refresh token missing.");
            }

            // compute hash of cookie value to compare with stored hashed tokens
            var refreshTokenHash = ComputeSha256Hash(refreshToken);

            _logger.LogDebug("Computed refreshTokenHash (first 8 chars): {HashPreview}", refreshTokenHash?.Substring(0, Math.Min(8, refreshTokenHash.Length)));

            // find stored token by hash
            var storedToken = await _context.UserRefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshTokenHash);

            if (storedToken == null)
            {
                _logger.LogWarning("No stored refresh token matched the provided hash.");
                return Unauthorized("Invalid refresh token.");
            }
            _logger.LogInformation("Found stored token. UserId={UserId} ExpiresAt={ExpiresAt} IsRevoked={IsRevoked}",
        storedToken.UserId, storedToken.ExpiresAt, storedToken.IsRevoked);

            if (storedToken.IsRevoked)
            {
                // Optional: revoke all tokens for this user (defense-in-depth)
                _logger.LogWarning("Refresh token is revoked for user {UserId}. Revoking all tokens.", storedToken.UserId);
                var userId = storedToken.UserId;
                var userTokens = await _context.UserRefreshTokens.Where(t => t.UserId == userId).ToListAsync();
                foreach (var t in userTokens) t.IsRevoked = true;
                await _context.SaveChangesAsync();

                return Unauthorized("This refresh token has been revoked. Please log in again.");
            }

            if (storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expired for user {UserId} ExpiresAt={ExpiresAt}", storedToken.UserId, storedToken.ExpiresAt);
                return Unauthorized("Refresh token expired. Please log in again.");
            }

            var user = storedToken.User;

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newRefreshTokenHash = ComputeSha256Hash(newRefreshToken);

            // mark old as revoked
            storedToken.IsRevoked = true;

            // add new token record (store hash, not raw)
            var newTokenEntry = new UserRefreshToken
            {
                Token = newRefreshTokenHash,
                JwtId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                UserId = user.Id
            };

            _context.UserRefreshTokens.Add(newTokenEntry);
            await _context.SaveChangesAsync();

            // set cookie (raw token in cookie so the client can present it later)
            Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(7)
            });

            _logger.LogInformation("Refresh success for user {UserId}. New token stored. Returning new access token.", user.Id);

            return Ok(new { AccessToken = newAccessToken });
        }

        [Authorize] // requires valid access token in Authorization header
        [HttpGet("validate-token")]
        public IActionResult ValidateToken()
        {
            
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.FindFirst("isAdmin")?.Value;
                var isMember = User.FindFirst("isMember")?.Value;

                return Ok(new { email, role,
                    id ,isAdmin,
                    isMember,
                });
            
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["RefreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var hash = ComputeSha256Hash(refreshToken);
                var storedToken = await _context.UserRefreshTokens.FirstOrDefaultAsync(rt => rt.Token == hash);
                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    await _context.SaveChangesAsync();
                }
            }

            // ✅ Overwrite both cookies with expired versions
            Response.Cookies.Append("AccessToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            });

            Response.Cookies.Append("RefreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            });

            return Ok(new { message = "Logged out successfully." });
        }





        [HttpGet("getUserByEmail/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound("User not found.");

            // Do not return password or navigation props
            var safe = new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.DateOfBirth
            };
            return Ok(safe);
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
            if (updatedUser == null) return BadRequest("Invalid payload.");
            var existingUser = await _context.Users.FindAsync(updatedUser.Id);
            if (existingUser == null) return NotFound("User not found.");

            // Optional: Check if email is changed and unique
            if (!string.Equals(existingUser.Email, updatedUser.Email, System.StringComparison.OrdinalIgnoreCase))
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == updatedUser.Email && u.Id != updatedUser.Id);
                if (emailExists) return Conflict("Email already in use.");
            }

            // Update fields
            existingUser.FullName = updatedUser.FullName;
            existingUser.Email = updatedUser.Email;

            await _context.SaveChangesAsync();

            var safe = new
            {
                existingUser.Id,
                existingUser.FullName,
                existingUser.Email,
                existingUser.Role
            };
            return Ok(safe);
        }

        private static string ComputeSha256Hash(string raw)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return Convert.ToBase64String(bytes);
        }
    }
}
