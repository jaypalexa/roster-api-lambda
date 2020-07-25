namespace RosterApiLambda.Models
{
    public class HomeSummaryModel
    {
        public int activeSeaTurtlesCount { get; set; }
        public int relinquishedSeaTurtlesCount { get; set; }
        public int acquiredHatchlingsEventsCount { get; set; }
        public int diedHatchlingsEventsCount { get; set; }
        public int releasedHatchlingsEventsCount { get; set; }
        public int doaHatchlingsEventsCount { get; set; }
        public int acquiredWashbacksEventsCount { get; set; }
        public int diedWashbacksEventsCount { get; set; }
        public int releasedWashbacksEventsCount { get; set; }
        public int doaWashbacksEventsCount { get; set; }
        public int holdingTanksCount { get; set; }
    }
}
