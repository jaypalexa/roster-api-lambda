using System.Collections.Generic;

namespace RosterApiLambda.Dtos.ReportResponses.HatchlingsAndWashbacksByCountyReport
{
    public class ContentDto
    {
        public ContentDto()
        {
            countyCounts = new List<CountyCountDto>();
        }

        public List<CountyCountDto> countyCounts { get; set; }
    }
}
