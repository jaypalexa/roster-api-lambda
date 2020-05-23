namespace RosterApiLambda.Models
{
    public class OrganizationModel
    {
        public string organizationId { get; set; }
        public string organizationName { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zipCode { get; set; }
        public string phone { get; set; }
        public string fax { get; set; }
        public string emailAddress { get; set; }
        public string permitNumber { get; set; }
        public string contactName { get; set; }
        public string preferredUnitsType { get; set; }
        public string hatchlingsBalanceAsOfDate { get; set; }
        public int ccHatchlingsStartingBalance { get; set; }
        public int cmHatchlingsStartingBalance { get; set; }
        public int dcHatchlingsStartingBalance { get; set; }
        public int otherHatchlingsStartingBalance { get; set; }
        public int unknownHatchlingsStartingBalance { get; set; }
        public string washbacksBalanceAsOfDate { get; set; }
        public int ccWashbacksStartingBalance { get; set; }
        public int cmWashbacksStartingBalance { get; set; }
        public int dcWashbacksStartingBalance { get; set; }
        public int otherWashbacksStartingBalance { get; set; }
        public int unknownWashbacksStartingBalance { get; set; }
    }
}
