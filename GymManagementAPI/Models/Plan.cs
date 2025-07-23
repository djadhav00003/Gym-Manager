namespace GymManagementAPI.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string PlanName { get; set; }
        public int DurationInDays { get; set; }
        public decimal Price { get; set; }
        public int GymId { get; set; }
       

        // ✅ New field
        public bool IsPersonalTrainerAvailable { get; set; }

        public int? TrainerId { get; set; }
        
    }
}
