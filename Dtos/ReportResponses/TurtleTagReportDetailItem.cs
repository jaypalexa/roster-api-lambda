using System.Collections.Generic;

namespace RosterApiLambda.Dtos.ReportResponses
{
    public class TurtleTagReportDetailItem
    {
        // seaTurtle.sidNumber
        // seaTurtle.seaTurtleName
        // seaTurtle.dateRelinquished
        // seaTurtle.seaTurtleTags (label (type (PIT) or location (LFF, etc); value (tagNumber); dateTagged)

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

    public class TurtleTagReportDetailItemTag
    {
        public string label { get; set; }       // label (type (PIT) or location (LFF | RFF | LRF | RRF))
        public string tagNumber { get; set; }
        public string dateTagged { get; set; }
    }
}
