using System.Collections.Generic;

namespace RosterApiLambda.Dtos.ReportResponses
{
    public class TurtleTagReportContent
    {
        public TurtleTagReportContent()
        {
            detailItems = new List<TurtleTagReportDetailItem>();
        }

        public List<TurtleTagReportDetailItem> detailItems { get; set; }
    }
}
