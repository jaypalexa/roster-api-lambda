using System.Collections.Generic;

namespace RosterApiLambda.Dtos.ReportResponses
{
    public class TurtleTagReportDetailItem
    {
        public TurtleTagReportDetailItem()
        {
            tags = new List<TurtleTagReportDetailItemTag>();
        }

        public string seaTurtleId { get; set; }
        public string sidNumber { get; set; }
        public string seaTurtleName { get; set; }
        public string dateRelinquished { get; set; }
        public string strandingIdNumber { get; set; }
        public List<TurtleTagReportDetailItemTag> tags { get; set; }
    }
}
