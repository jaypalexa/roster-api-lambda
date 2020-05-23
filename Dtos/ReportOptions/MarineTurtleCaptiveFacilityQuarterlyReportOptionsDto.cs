using System;
using System.Collections.Generic;
using System.Text;

namespace RosterApiLambda.Dtos.ReportOptions
{
    public class MarineTurtleCaptiveFacilityQuarterlyReportOptionsDto
    {
        public string subjectType { get; set; }     // Hatchlings | Washbacks
        public string dateFrom { get; set; }
        public string dateThru { get; set; }
        public string comments { get; set; }
        public bool includeDoaCounts { get; set; }
    }
}
