using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;

namespace RosterApiLambda.Services
{
    public class SeaTurtleMorphometricService
    {
        private DataHelper _dataHelper { get; }
        private string _pk { get; }

        public SeaTurtleMorphometricService(string organizationId, string seaTurtleId)
        {
            _dataHelper = new DataHelper(organizationId);
            _pk = $"SEA_TURTLE#{seaTurtleId}";
        }

        public async Task<List<string>> GetSeaTurtleMorphometrics()
            => await _dataHelper.QueryAsync(_pk, "SEA_TURTLE_MORPHOMETRIC#");

        public async Task<string> GetSeaTurtleMorphometric(string seaTurtleMorphometricId)
            => await _dataHelper.GetItemAsync(_pk, $"SEA_TURTLE_MORPHOMETRIC#{seaTurtleMorphometricId}");

        public async Task<PutItemResponse> SaveSeaTurtleMorphometric(string seaTurtleMorphometricId, string body)
            => await _dataHelper.PutItemAsync(_pk, $"SEA_TURTLE_MORPHOMETRIC#{seaTurtleMorphometricId}", body);

        public async Task<DeleteItemResponse> DeleteSeaTurtleMorphometric(string seaTurtleMorphometricId)
            => await _dataHelper.DeleteItemAsync(_pk, $"SEA_TURTLE_MORPHOMETRIC#{seaTurtleMorphometricId}");
    }
}
