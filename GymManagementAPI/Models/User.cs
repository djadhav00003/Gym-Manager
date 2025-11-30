namespace GymManagementAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }  // "Admin", "Member", "Trainer"

        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }

        // ✅ One User → Many Refresh Tokens
        public ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();

        // ✅ Optional One User → One Member
        public ICollection<Member> Members { get; set; } = new List<Member>();

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    }


    public class UserResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }  // Also acts as "Email Address"
        public string Password { get; set; }
        public string Role { get; set; }  // "Admin", "Member", "Trainer"

        // New Fields
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }

        public Boolean? isMember { get; set; }
    }
}