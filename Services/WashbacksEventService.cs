using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;

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

        public async Task<List<string>> GetWashbacksEvents()
            => await _dataHelper.QueryAsync(_pk, "WASHBACKS_EVENT#");

        public async Task<string> GetWashbacksEvent(string washbacksEventId)
            => await _dataHelper.GetItemAsync(_pk, $"WASHBACKS_EVENT#{washbacksEventId}");

        public async Task<PutItemResponse> SaveWashbacksEvent(string washbacksEventId, string body)
            => await _dataHelper.PutItemAsync(_pk, $"WASHBACKS_EVENT#{washbacksEventId}", body);

        public async Task<DeleteItemResponse> DeleteWashbacksEvent(string washbacksEventId)
            => await _dataHelper.DeleteItemAsync(_pk, $"WASHBACKS_EVENT#{washbacksEventId}");
    }
}
