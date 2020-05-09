using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;

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

        public async Task<List<string>> GetHatchlingsEvents()
            => await _dataHelper.QueryAsync(_pk, "HATCHLINGS_EVENT#");

        public async Task<string> GetHatchlingsEvent(string hatchlingsEventId)
            => await _dataHelper.GetItemAsync(_pk, $"HATCHLINGS_EVENT#{hatchlingsEventId}");

        public async Task<PutItemResponse> SaveHatchlingsEvent(string hatchlingsEventId, string body)
            => await _dataHelper.PutItemAsync(_pk, $"HATCHLINGS_EVENT#{hatchlingsEventId}", body);

        public async Task<DeleteItemResponse> DeleteHatchlingsEvent(string hatchlingsEventId)
            => await _dataHelper.DeleteItemAsync(_pk, $"HATCHLINGS_EVENT#{hatchlingsEventId}");
    }
}
