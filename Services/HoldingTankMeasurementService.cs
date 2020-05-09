using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;

namespace RosterApiLambda.Services
{
    public class HoldingTankMeasurementService
    {
        private DataHelper _dataHelper { get; }
        private string _pk { get; }

        public HoldingTankMeasurementService(string organizationId, string holdingTankId)
        {
            _dataHelper = new DataHelper(organizationId);
            _pk = $"HOLDING_TANK#{holdingTankId}";
        }

        public async Task<List<string>> GetHoldingTankMeasurements()
            => await _dataHelper.QueryAsync(_pk, "HOLDING_TANK_MEASUREMENT#");

        public async Task<string> GetHoldingTankMeasurement(string holdingTankMeasurementId)
            => await _dataHelper.GetItemAsync(_pk, $"HOLDING_TANK_MEASUREMENT#{holdingTankMeasurementId}");

        public async Task<PutItemResponse> SaveHoldingTankMeasurement(string holdingTankMeasurementId, string body)
            => await _dataHelper.PutItemAsync(_pk, $"HOLDING_TANK_MEASUREMENT#{holdingTankMeasurementId}", body);

        public async Task<DeleteItemResponse> DeleteHoldingTankMeasurement(string holdingTankMeasurementId)
            => await _dataHelper.DeleteItemAsync(_pk, $"HOLDING_TANK_MEASUREMENT#{holdingTankMeasurementId}");
    }
}
