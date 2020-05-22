using System.Collections.Generic;

namespace RosterApiLambda.Dtos.ReportResponses
{
    public class HatchlingsAndWashbacksByCountyReportContentDto
    {
        public HatchlingsAndWashbacksByCountyReportContentDto()
        {
            countyCounts = new List<HatchlingsAndWashbacksByCountyReportCountyCountDto>();
        }

        public List<HatchlingsAndWashbacksByCountyReportCountyCountDto> countyCounts { get; set; }
    }
}
