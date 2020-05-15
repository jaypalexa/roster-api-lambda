namespace RosterApiLambda.Models
{
    public class HatchlingsEventModel
    {
        public string hatchlingsEventId { get; set; }
        public string organizationId { get; set; }
        public string eventType { get; set; }
        public string species { get; set; }
        public string eventDate { get; set; }
        public int eventCount { get; set; }
        public int beachEventCount { get; set; }
        public int offshoreEventCount { get; set; }
        public string eventCounty { get; set; }
    }
}
