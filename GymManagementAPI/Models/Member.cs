namespace GymManagementAPI.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string MemberName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }

        // Foreign keys
        public int GymId { get; set; }
        public Gym Gym { get; set; }

        public int? TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int PlanId { get; set; }
        public Plan Plan { get; set; }

        // ✅ One Member → Many Payments

    }

    public class MemberCreateDto
    {
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public int GymId { get; set; }
        public int? TrainerId { get; set; }
    }
}
