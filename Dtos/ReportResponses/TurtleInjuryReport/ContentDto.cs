using System.Collections.Generic;

namespace RosterApiLambda.Dtos.ReportResponses.TurtleInjuryReport
{
    public class ContentDto
    {
        public ContentDto()
        {
            summaryItems = new List<SummaryItemDto>();
            detailItems = new List<DetailItemDto>();
        }

        public int totalCount { get; set; }
        public List<SummaryItemDto> summaryItems { get; set; }
        public List<DetailItemDto> detailItems { get; set; }
    }
}
