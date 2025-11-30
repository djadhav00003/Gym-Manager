namespace GymManagementAPI.Models
{
    public class AuthResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public UserResponse User { get; set; }
    }
}