namespace GymManagementAPI.Models
{
    public class GymImages
    {
        public int Id { get; set; }
        public int GymId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.Now;
        public Gym Gym { get; set; }
    }
}
