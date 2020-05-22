namespace RosterApiLambda.Dtos.ReportResponses
{
    public class HatchlingsAndWashbacksByCountyReportPercentageOfGrandTotalDto
    {
        public double hatchlingsAcquired { get; set; }
        public double hatchlingsDoa { get; set; }
        public double washbacksUnder5cmAcquired { get; set; }
        public double washbacksOver5cmAcquired { get; set; }
        public double washbacksUnder5cmDoa { get; set; }
        public double washbacksOver5cmDoa { get; set; }

        public void AppendCounts(HatchlingsAndWashbacksByCountyReportDetailItemDto from)
        {
            hatchlingsAcquired += from.hatchlingsAcquired;
            hatchlingsDoa += from.hatchlingsDoa;
            washbacksUnder5cmAcquired += from.washbacksUnder5cmAcquired;
            washbacksOver5cmAcquired += from.washbacksOver5cmAcquired;
            washbacksUnder5cmDoa += from.washbacksUnder5cmDoa;
            washbacksOver5cmDoa += from.washbacksOver5cmDoa;
        }
    }
}
