namespace GymManagementAPI.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public string TrainerName { get; set; }
        public string Speciality { get; set; }
        public int Experience { get; set; }           // ✅ New field
        public string PhoneNumber { get; set; }       // ✅ New field
        public int GymId { get; set; }
    }
}