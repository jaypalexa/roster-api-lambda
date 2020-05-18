using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;

namespace RosterApiLambda.Services
{
    public class HoldingTankService
    {
        private DataHelper _dataHelper { get; }
        private string _pk { get; }
        private string _organizationId { get; }

        public HoldingTankService(string organizationId)
        {
            _dataHelper = new DataHelper(organizationId);
            _pk = $"ORGANIZATION#{organizationId}";
            _organizationId = organizationId;
        }

        public async Task<List<HoldingTankModel>> GetHoldingTanks()
            => await _dataHelper.QueryAsync<HoldingTankModel>(_pk, "HOLDING_TANK#");

        public async Task<HoldingTankModel> GetHoldingTank(string holdingTankId)
            => await _dataHelper.GetItemAsync<HoldingTankModel>(_pk, $"HOLDING_TANK#{holdingTankId}");

        public async Task<PutItemResponse> SaveHoldingTank(string holdingTankId, HoldingTankModel holdingTank)
            => await _dataHelper.PutItemAsync(_pk, $"HOLDING_TANK#{holdingTankId}", holdingTank);

        public async Task<DeleteItemResponse> DeleteHoldingTank(string holdingTankId)
        {
            var holdingTankMeasurementService = new HoldingTankMeasurementService(_organizationId, holdingTankId);

            var holdingTankMeasurements = await holdingTankMeasurementService.GetHoldingTankMeasurements();
            foreach (var holdingTankMeasurement in holdingTankMeasurements)
            {
                await holdingTankMeasurementService.DeleteHoldingTankMeasurement(holdingTankMeasurement.holdingTankMeasurementId);
            }

            return await _dataHelper.DeleteItemAsync(_pk, $"HOLDING_TANK#{holdingTankId}");
        }
    }
}
