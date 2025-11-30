namespace GymManagementAPI.Models
{
    public class Gym
    {
        public int Id { get; set; }
        public string GymName { get; set; }
        public string Location { get; set; }
        public string Contact { get; set; }

        // Comma-separated facilities list
        public string Facilities { get; set; }

        // ✅ Navigation: One Gym → Many Trainers
        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();

        // ✅ Navigation: One Gym → Many Plans
        public ICollection<Plan> Plans { get; set; } = new List<Plan>();

        // ✅ Navigation: One Gym → Many Members
        public ICollection<Member> Members { get; set; } = new List<Member>();
        public ICollection<GymImages> GymImages { get; set; } = new List<GymImages>();

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }
        public User? DeletedByUser { get; set; }
        public int OwnerUserId { get; set; }
        public User? OwnerUser { get; set; }

    }

    public class GymWithTrainerCountDto
    {
        public int Id { get; set; }
        public string GymName { get; set; }
        public string Location { get; set; }
        public string Contact { get; set; }
        public string Facilities { get; set; }
        public int TrainerCount { get; set; }

    }

    public class GymImagesDto
    {
        public List<string> ImageUrls { get; set; }
    }
}