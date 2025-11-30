namespace GymManagementAPI.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public string TrainerName { get; set; }
        public string Speciality { get; set; }
        public int Experience { get; set; }
        public string PhoneNumber { get; set; }

        // Foreign key
        public int GymId { get; set; }
        public Gym? Gym { get; set; }

        // ✅ One Trainer → Many Plans
        public ICollection<Plan> Plans { get; set; } = new List<Plan>();

        // ✅ One Trainer → Many Members
        public ICollection<Member> Members { get; set; } = new List<Member>();
    }
}