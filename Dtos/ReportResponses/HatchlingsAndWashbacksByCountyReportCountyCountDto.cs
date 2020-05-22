namespace RosterApiLambda.Dtos.ReportResponses
{
    public class HatchlingsAndWashbacksByCountyReportCountyCountDto
    {
        public HatchlingsAndWashbacksByCountyReportCountyCountDto()
        {
            ccCount = new HatchlingsAndWashbacksByCountyReportDetailItemDto();
            cmCount = new HatchlingsAndWashbacksByCountyReportDetailItemDto();
            dcCount = new HatchlingsAndWashbacksByCountyReportDetailItemDto();
            otherCount = new HatchlingsAndWashbacksByCountyReportDetailItemDto();
            unknownCount = new HatchlingsAndWashbacksByCountyReportDetailItemDto();
            totalCount = new HatchlingsAndWashbacksByCountyReportDetailItemDto();
            percentageOfGrandTotal = new HatchlingsAndWashbacksByCountyReportPercentageOfGrandTotalDto();
        }

        public string countyName { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto ccCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto cmCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto dcCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto otherCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto unknownCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto totalCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportPercentageOfGrandTotalDto percentageOfGrandTotal { get; set; }

        public void AppendCounts(HatchlingsAndWashbacksByCountyReportCountyCountDto from)
        {
            ccCount.AppendCounts(from.ccCount);
            cmCount.AppendCounts(from.cmCount);
            dcCount.AppendCounts(from.dcCount);
            otherCount.AppendCounts(from.otherCount);
            unknownCount.AppendCounts(from.unknownCount);
            totalCount.AppendCounts(from.totalCount);
        }

        public void SetPercentageOfGrandTotal(HatchlingsAndWashbacksByCountyReportDetailItemDto grandTotalCount)
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
