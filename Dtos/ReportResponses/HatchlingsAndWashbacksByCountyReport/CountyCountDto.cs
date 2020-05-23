namespace RosterApiLambda.Dtos.ReportResponses.HatchlingsAndWashbacksByCountyReport
{
    public class CountyCountDto
    {
        public CountyCountDto()
        {
            ccCount = new DetailItemDto();
            cmCount = new DetailItemDto();
            dcCount = new DetailItemDto();
            otherCount = new DetailItemDto();
            unknownCount = new DetailItemDto();
            totalCount = new DetailItemDto();
            percentageOfGrandTotal = new PercentageOfGrandTotalDto();
        }

        public string countyName { get; set; }
        public DetailItemDto ccCount { get; set; }
        public DetailItemDto cmCount { get; set; }
        public DetailItemDto dcCount { get; set; }
        public DetailItemDto otherCount { get; set; }
        public DetailItemDto unknownCount { get; set; }
        public DetailItemDto totalCount { get; set; }
        public PercentageOfGrandTotalDto percentageOfGrandTotal { get; set; }

        public void AppendCounts(CountyCountDto from)
        {
            ccCount.AppendCounts(from.ccCount);
            cmCount.AppendCounts(from.cmCount);
            dcCount.AppendCounts(from.dcCount);
            otherCount.AppendCounts(from.otherCount);
            unknownCount.AppendCounts(from.unknownCount);
            totalCount.AppendCounts(from.totalCount);
        }

        public void SetPercentageOfGrandTotal(DetailItemDto grandTotalCount)
        {
            percentageOfGrandTotal.hatchlingsAcquired = (grandTotalCount.hatchlingsAcquired == 0) ? 0 : ((double)totalCount.hatchlingsAcquired / grandTotalCount.hatchlingsAcquired * 100);
            percentageOfGrandTotal.hatchlingsDoa = (grandTotalCount.hatchlingsDoa == 0) ? 0 : ((double)totalCount.hatchlingsDoa / grandTotalCount.hatchlingsDoa * 100);
            percentageOfGrandTotal.washbacksUnder5cmAcquired = (grandTotalCount.washbacksUnder5cmAcquired == 0) ? 0 : ((double)totalCount.washbacksUnder5cmAcquired / grandTotalCount.washbacksUnder5cmAcquired * 100);
            percentageOfGrandTotal.washbacksOver5cmAcquired = (grandTotalCount.washbacksOver5cmAcquired == 0) ? 0 : ((double)totalCount.washbacksOver5cmAcquired / grandTotalCount.washbacksOver5cmAcquired * 100);
            percentageOfGrandTotal.washbacksUnder5cmDoa = (grandTotalCount.washbacksUnder5cmDoa == 0) ? 0 : ((double)totalCount.washbacksUnder5cmDoa / grandTotalCount.washbacksUnder5cmDoa * 100);
            percentageOfGrandTotal.washbacksOver5cmDoa = (grandTotalCount.washbacksOver5cmDoa == 0) ? 0 : ((double)totalCount.washbacksOver5cmDoa / grandTotalCount.washbacksOver5cmDoa * 100);
        }
    }
}
