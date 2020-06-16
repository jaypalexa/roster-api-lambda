namespace RosterApiLambda.Models
{
    public class LogEntryModel
    {
        public string logEntryId { get; set; }      // FROM SERVER:  NOW AS UTC = yyyy-MM-dd-HH-mm-ss-fff
        public string organizationId { get; set; }  // FROM CLIENT
        public string userName { get; set; }        // FROM CLIENT
        public string entryDateTime { get; set; }   // FROM SERVER:  NOW AS UTC == yyyy-MM-dd HH:mm:ssZ
        public string message { get; set; }         // FROM CLIENT
        public int timestamp { get; set; }          // FROM SERVER:  NOWUNIX TIMESTAMP IN SECONDS
    }
}
