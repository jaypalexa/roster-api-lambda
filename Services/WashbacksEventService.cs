using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;

namespace RosterApiLambda.Services
{
    public class WashbacksEventService
    {
        private DataHelper _dataHelper { get; }
        private string _pk { get; }

        public WashbacksEventService(string organizationId)
        {
            _dataHelper = new DataHelper(organizationId);
            _pk = $"ORGANIZATION#{organizationId}";
        }

        public async Task<List<WashbacksEventModel>> GetWashbacksEvents()
            => await _dataHelper.QueryAsync<WashbacksEventModel>(_pk, "WASHBACKS_EVENT#");

        public async Task<WashbacksEventModel> GetWashbacksEvent(string washbacksEventId)
            => await _dataHelper.GetItemAsync<WashbacksEventModel>(_pk, $"WASHBACKS_EVENT#{washbacksEventId}");

        public async Task<PutItemResponse> SaveWashbacksEvent(string washbacksEventId, WashbacksEventModel washbacksEvent)
            => await _dataHelper.PutItemAsync(_pk, $"WASHBACKS_EVENT#{washbacksEventId}", washbacksEvent);

        public async Task<DeleteItemResponse> DeleteWashbacksEvent(string washbacksEventId)
            => await _dataHelper.DeleteItemAsync(_pk, $"WASHBACKS_EVENT#{washbacksEventId}");
    }
}
