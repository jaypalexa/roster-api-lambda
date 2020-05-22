namespace RosterApiLambda.Dtos.ReportResponses
{
    public class TurtleTagReportDetailItemTag
    {
        public string label { get; set; }       // label (type (PIT) or location (LFF | RFF | LRF | RRF))
        public string tagNumber { get; set; }
        public string dateTagged { get; set; }
    }
}
