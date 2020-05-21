namespace RosterApiLambda.Dtos.ReportOptions
{
    public class TurtleTagReportOptions
    {
        public string dateFrom { get; set; }
        public string dateThru { get; set; }
        public string filterDateType { get; set; } // dateAcquired | dateTagged | dateRelinquished
        public bool includeNonRelinquishedTurtles { get; set; }
        public bool includeStrandingIdNumber { get; set; }
        public bool isPit { get; set; }
        public bool isLff { get; set; }
        public bool isRff { get; set; }
        public bool isLrf { get; set; }
        public bool isRrf { get; set; }
    }
}
