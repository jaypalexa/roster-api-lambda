namespace RosterApiLambda.Dtos.ReportOptions
{
    public class TaggingDataFormReportOptionsDto
    {
        public string seaTurtleId { get; set; }
        public bool populateFacilityField { get; set; }
        public bool printSidOnForm { get; set; }
        public bool additionalRemarksOrDataOnBackOfForm { get; set; }
        public string useMorphometricsClosestTo { get; set; }
    }
}
