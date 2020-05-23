namespace RosterApiLambda.Dtos.ReportResponses.TurtleTagReport
{
    public class DetailItemTagDto
    {
        public string label { get; set; }       // label (type (PIT) or location (LFF | RFF | LRF | RRF))
        public string tagNumber { get; set; }
        public string dateTagged { get; set; }
    }
}
