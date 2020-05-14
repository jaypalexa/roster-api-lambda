namespace RosterApiLambda.Models
{
    public class SeaTurtleModel
    {
        public string seaTurtleId { get; set; }
        public string organizationId { get; set; }
        public string seaTurtleName { get; set; }
        public string sidNumber { get; set; }
        public string strandingIdNumber { get; set; }
        public string species { get; set; }
        public string dateCaptured { get; set; }
        public string dateAcquired { get; set; }
        public string acquiredFrom { get; set; }
        public string turtleSize { get; set; }
        public string status { get; set; }
        public string dateRelinquished { get; set; }
        public string relinquishedTo { get; set; }
        public string anomalies { get; set; }
        public bool injuryBoatStrike { get; set; }
        public bool injuryIntestinalImpaction { get; set; }
        public bool injuryLineEntanglement { get; set; }
        public bool injuryFishHook { get; set; }
        public bool injuryUpperRespiratory { get; set; }
        public bool injuryAnimalBite { get; set; }
        public bool injuryFibropapilloma { get; set; }
        public bool injuryMiscEpidemic { get; set; }
        public bool injuryDoa { get; set; }
        public bool injuryOther { get; set; }
        public bool wasCarryingTagsWhenEnc { get; set; }
        public string recaptureType { get; set; }
        public string tagReturnAddress { get; set; }
        public string captureProjectType { get; set; }
        public string didTurtleNest { get; set; }
        public string captureProjectOther { get; set; }
        public string acquiredCounty { get; set; }
        public string acquiredLatitude { get; set; }
        public string acquiredLongitude { get; set; }
        public string relinquishedCounty { get; set; }
        public string relinquishedLatitude { get; set; }
        public string relinquishedLongitude { get; set; }
        public bool inspectedForTagScars { get; set; }
        public string tagScarsLocated { get; set; }
        public bool scannedForPitTags { get; set; }
        public string pitTagsScanFrequency { get; set; }
        public bool scannedForMagneticWires { get; set; }
        public string magneticWiresLocated { get; set; }
        public bool inspectedForLivingTags { get; set; }
        public string livingTagsLocated { get; set; }
        public string brochureComments { get; set; }
        public string brochureBackgroundColor { get; set; }
        public string brochureImageFileAttachmentId { get; set; }
    }
}
