using System;
using System.Collections.Generic;
using System.Text;

namespace RosterApiLambda.Dtos.ReportOptions
{
    public class MarineTurtleHoldingFacilityQuarterlyReportOptionsDto
    {
        public string dateFrom { get; set; }
        public string dateThru { get; set; }
        public string groupTankDataBy { get; set; }     // tank | date
        public bool includeAnomalies { get; set; }
        public bool includeAcquiredFrom { get; set; }
        public bool includeTurtleName { get; set; }
    }
}
