namespace RosterApiLambda.Dtos.ReportResponses
{
    public class HatchlingsAndWashbacksByCountyReportDetailItemDto
    {
        public int hatchlingsAcquired { get; set; }
        public int hatchlingsDoa { get; set; }
        public int washbacksUnder5cmAcquired { get; set; }
        public int washbacksOver5cmAcquired { get; set; }
        public int washbacksUnder5cmDoa { get; set; }
        public int washbacksOver5cmDoa { get; set; }
    }
}
