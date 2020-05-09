using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;

namespace RosterApiLambda.Services
{
    public class SeaTurtleTagService
    {
        private DataHelper _dataHelper { get; }
        private string _pk { get; }

        public SeaTurtleTagService(string organizationId, string seaTurtleId)
        {
            _dataHelper = new DataHelper(organizationId);
            _pk = $"SEA_TURTLE#{seaTurtleId}";
        }

        public async Task<List<string>> GetSeaTurtleTags()
            => await _dataHelper.QueryAsync(_pk, "SEA_TURTLE_TAG#");

        public async Task<string> GetSeaTurtleTag(string seaTurtleTagId)
            => await _dataHelper.GetItemAsync(_pk, $"SEA_TURTLE_TAG#{seaTurtleTagId}");

        public async Task<PutItemResponse> SaveSeaTurtleTag(string seaTurtleTagId, string body)
            => await _dataHelper.PutItemAsync(_pk, $"SEA_TURTLE_TAG#{seaTurtleTagId}", body);

        public async Task<DeleteItemResponse> DeleteSeaTurtleTag(string seaTurtleTagId)
            => await _dataHelper.DeleteItemAsync(_pk, $"SEA_TURTLE_TAG#{seaTurtleTagId}");
    }
}
