namespace GymManagementAPI.Models
{
    public class Gym
    {
        public int Id { get; set; }
        public string GymName { get; set; }
        public string Location { get; set; }
        public string Contact { get; set; }

        // New field for storing comma-separated facilities
        public string Facilities { get; set; }
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
}