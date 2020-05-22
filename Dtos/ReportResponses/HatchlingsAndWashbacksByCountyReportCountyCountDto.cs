namespace RosterApiLambda.Dtos.ReportResponses
{
    public class HatchlingsAndWashbacksByCountyReportCountyCountDto
    {
        public string countyName { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto ccCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto cmCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto dcCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto otherCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto unknownCount { get; set; }
        public HatchlingsAndWashbacksByCountyReportDetailItemDto totalCount { get; set; }

    }
}
