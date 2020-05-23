namespace RosterApiLambda.Dtos.ReportResponses.TurtleInjuryReport
{
    public class DetailItemDto
    {
        public string seaTurtleId { get; set; }
        public string seaTurtleName { get; set; }
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
    }
}
