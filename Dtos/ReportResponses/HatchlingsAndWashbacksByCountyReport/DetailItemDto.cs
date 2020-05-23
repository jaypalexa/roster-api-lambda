namespace RosterApiLambda.Dtos.ReportResponses.HatchlingsAndWashbacksByCountyReport
{
    public class DetailItemDto
    {
        public int hatchlingsAcquired { get; set; }
        public int hatchlingsDoa { get; set; }
        public int washbacksUnder5cmAcquired { get; set; }
        public int washbacksOver5cmAcquired { get; set; }
        public int washbacksUnder5cmDoa { get; set; }
        public int washbacksOver5cmDoa { get; set; }

        public void AppendCounts(DetailItemDto from)
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
