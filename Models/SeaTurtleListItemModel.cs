namespace RosterApiLambda.Models
{
    public class SeaTurtleListItemModel
    {
        public string seaTurtleId { get; set; }
        public string organizationId { get; set; }
        public string seaTurtleName { get; set; }
        public string sidNumber { get; set; }
        public string species { get; set; }
        public string dateAcquired { get; set; }
        public string acquiredCounty { get; set; }
        public string turtleSize { get; set; }
        public string status { get; set; }
        public string dateRelinquished { get; set; }
    }
}
