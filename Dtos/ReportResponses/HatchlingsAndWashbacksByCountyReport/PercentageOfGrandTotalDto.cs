namespace RosterApiLambda.Dtos.ReportResponses.HatchlingsAndWashbacksByCountyReport
{
    public class PercentageOfGrandTotalDto
    {
        public double hatchlingsAcquired { get; set; }
        public double hatchlingsDoa { get; set; }
        public double washbacksUnder5cmAcquired { get; set; }
        public double washbacksOver5cmAcquired { get; set; }
        public double washbacksUnder5cmDoa { get; set; }
        public double washbacksOver5cmDoa { get; set; }
    }
}
