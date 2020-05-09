using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;

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

        public async Task<List<string>> GetHoldingTanks()
            => await _dataHelper.QueryAsync(_pk, "HOLDING_TANK#");

        public async Task<string> GetHoldingTank(string holdingTankId)
            => await _dataHelper.GetItemAsync(_pk, $"HOLDING_TANK#{holdingTankId}");

        public async Task<PutItemResponse> SaveHoldingTank(string holdingTankId, string body)
            => await _dataHelper.PutItemAsync(_pk, $"HOLDING_TANK#{holdingTankId}", body);

        public async Task<DeleteItemResponse> DeleteHoldingTank(string holdingTankId)
        {
            var holdingTankMeasurementService = new HoldingTankMeasurementService(_organizationId, holdingTankId);

            var holdingTankMeasurements = await holdingTankMeasurementService.GetHoldingTankMeasurements();
            foreach (var holdingTankMeasurementAsJson in holdingTankMeasurements)
            {
                dynamic holdingTankMeasurement = JsonSerializer.Deserialize<ExpandoObject>(holdingTankMeasurementAsJson);
                await holdingTankMeasurementService.DeleteHoldingTankMeasurement(holdingTankMeasurement.holdingTankMeasurementId.GetString());
            }

            return await _dataHelper.DeleteItemAsync(_pk, $"HOLDING_TANK#{holdingTankId}");
        }
    }
}
