using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;

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

        public async Task<List<HoldingTankMeasurementModel>> GetHoldingTankMeasurements()
            => await _dataHelper.QueryAsync<HoldingTankMeasurementModel>(_pk, "HOLDING_TANK_MEASUREMENT#");

        public async Task<HoldingTankMeasurementModel> GetHoldingTankMeasurement(string holdingTankMeasurementId)
            => await _dataHelper.GetItemAsync<HoldingTankMeasurementModel>(_pk, $"HOLDING_TANK_MEASUREMENT#{holdingTankMeasurementId}");

        public async Task<PutItemResponse> SaveHoldingTankMeasurement(string holdingTankMeasurementId, HoldingTankMeasurementModel holdingTankMeasurement)
            => await _dataHelper.PutItemAsync(_pk, $"HOLDING_TANK_MEASUREMENT#{holdingTankMeasurementId}", holdingTankMeasurement);

        public async Task<DeleteItemResponse> DeleteHoldingTankMeasurement(string holdingTankMeasurementId)
            => await _dataHelper.DeleteItemAsync(_pk, $"HOLDING_TANK_MEASUREMENT#{holdingTankMeasurementId}");
    }
}
