namespace GymManagementAPI.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string MemberName { get; set; }
        public int Age { get; set; }
        public string Email { get; set; }

        public int GymId { get; set; }
        public int? TrainerId { get; set; }

        // ✅ Just a plain integer column, no foreign key relationship
        public int UserId { get; set; }
        public int PlanId { get; set; }
    }

    public class MemberCreateDto
    {
        public int UserId { get; set; }
        public int PlanId { get; set; }
        public int GymId { get; set; }
        public int? TrainerId { get; set; }
    }
}
