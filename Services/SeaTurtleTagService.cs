using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;

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

        public async Task<List<SeaTurtleTagModel>> GetSeaTurtleTags()
            => await _dataHelper.QueryAsync<SeaTurtleTagModel>(_pk, "SEA_TURTLE_TAG#");

        public async Task<SeaTurtleTagModel> GetSeaTurtleTag(string seaTurtleTagId)
            => await _dataHelper.GetItemAsync<SeaTurtleTagModel>(_pk, $"SEA_TURTLE_TAG#{seaTurtleTagId}");

        public async Task<PutItemResponse> SaveSeaTurtleTag(string seaTurtleTagId, SeaTurtleTagModel seaTurtleTagModel)
            => await _dataHelper.PutItemAsync(_pk, $"SEA_TURTLE_TAG#{seaTurtleTagId}", seaTurtleTagModel);

        public async Task<DeleteItemResponse> DeleteSeaTurtleTag(string seaTurtleTagId)
            => await _dataHelper.DeleteItemAsync(_pk, $"SEA_TURTLE_TAG#{seaTurtleTagId}");
    }
}
