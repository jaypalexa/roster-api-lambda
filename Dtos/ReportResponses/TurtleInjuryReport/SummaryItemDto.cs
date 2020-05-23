namespace RosterApiLambda.Dtos.ReportResponses.TurtleInjuryReport
{
    public class SummaryItemDto
    {
        public string label { get; set; }
        public int count { get; set; }
        public double percentageOfTotal { get; set; }
    }
}
