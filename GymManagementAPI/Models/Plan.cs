namespace GymManagementAPI.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string PlanName { get; set; }
        public int DurationInDays { get; set; }
        public decimal Price { get; set; }

        public bool IsPersonalTrainerAvailable { get; set; }

        // Foreign keys
        public int GymId { get; set; }
        public Gym? Gym { get; set; }

        public int? TrainerId { get; set; }
        public Trainer? Trainer { get; set; }

        // ✅ One Plan → Many Members
        public ICollection<Member> Members { get; set; } = new List<Member>();

        // ✅ One Plan → Many Payments
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }

}
