namespace RosterApiLambda.Models
{
    public class HoldingTankMeasurementModel
    {
        public string holdingTankMeasurementId { get; set; }
        public string holdingTankId { get; set; }
        public string dateMeasured { get; set; }
        public double temperature { get; set; }
        public double salinity { get; set; }
        public double ph { get; set; }
    }
}
