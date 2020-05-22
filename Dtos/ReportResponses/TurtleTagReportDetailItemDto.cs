using System.Collections.Generic;

namespace RosterApiLambda.Dtos.ReportResponses
{
    public class TurtleTagReportDetailItemDto
    {
        public TurtleTagReportDetailItemDto()
        {
            tags = new List<TurtleTagReportDetailItemTagDto>();
        }

        public string seaTurtleId { get; set; }
        public string sidNumber { get; set; }
        public string seaTurtleName { get; set; }
        public string dateRelinquished { get; set; }
        public string strandingIdNumber { get; set; }
        public List<TurtleTagReportDetailItemTagDto> tags { get; set; }
    }
}
