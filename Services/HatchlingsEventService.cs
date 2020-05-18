using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;

namespace RosterApiLambda.Services
{
    public class HatchlingsEventService
    {
        private DataHelper _dataHelper { get; }
        private string _pk { get; }

        public HatchlingsEventService(string organizationId)
        {
            _dataHelper = new DataHelper(organizationId);
            _pk = $"ORGANIZATION#{organizationId}";
        }

        public async Task<List<HatchlingsEventModel>> GetHatchlingsEvents()
            => await _dataHelper.QueryAsync<HatchlingsEventModel>(_pk, "HATCHLINGS_EVENT#");

        public async Task<HatchlingsEventModel> GetHatchlingsEvent(string hatchlingsEventId)
            => await _dataHelper.GetItemAsync<HatchlingsEventModel>(_pk, $"HATCHLINGS_EVENT#{hatchlingsEventId}");

        public async Task<PutItemResponse> SaveHatchlingsEvent(string hatchlingsEventId, HatchlingsEventModel hatchlingsEvent)
            => await _dataHelper.PutItemAsync(_pk, $"HATCHLINGS_EVENT#{hatchlingsEventId}", hatchlingsEvent);

        public async Task<DeleteItemResponse> DeleteHatchlingsEvent(string hatchlingsEventId)
            => await _dataHelper.DeleteItemAsync(_pk, $"HATCHLINGS_EVENT#{hatchlingsEventId}");
    }
}
