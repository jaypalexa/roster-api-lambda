using System.Collections.Generic;

namespace RosterApiLambda.Dtos.ReportResponses
{
    public class TurtleTagReportContentDto
    {
        public TurtleTagReportContentDto()
        {
            detailItems = new List<TurtleTagReportDetailItemDto>();
        }

        public List<TurtleTagReportDetailItemDto> detailItems { get; set; }
    }
}
