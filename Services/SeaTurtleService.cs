using System.Collections.Generic;
using System.Dynamic;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;

namespace RosterApiLambda.Services
{
    public class SeaTurtleService
    {
        private DataHelper _dataHelper { get; }
        private string _pk { get; }
        private string _organizationId { get; }

        public SeaTurtleService(string organizationId)
        {
            _dataHelper = new DataHelper(organizationId);
            _pk = $"ORGANIZATION#{organizationId}";
            _organizationId = organizationId;
        }

        public async Task<List<string>> GetSeaTurtles()
            => await _dataHelper.QueryAsync(_pk, "SEA_TURTLE#");

        public async Task<string> GetSeaTurtle(string seaTurtleId)
            => await _dataHelper.GetItemAsync(_pk, $"SEA_TURTLE#{seaTurtleId}");

        public async Task<PutItemResponse> SaveSeaTurtle(string seaTurtleId, string body)
            => await _dataHelper.PutItemAsync(_pk, $"SEA_TURTLE#{seaTurtleId}", body);

        public async Task<DeleteItemResponse> DeleteSeaTurtle(string seaTurtleId)
        {
            var seaTurtleTagService = new SeaTurtleTagService(_organizationId, seaTurtleId);

            var seaTurtleTags = await seaTurtleTagService.GetSeaTurtleTags();
            foreach (var seaTurtleTagAsJson in seaTurtleTags)
            {
                dynamic seaTurtleTag = JsonSerializer.Deserialize<ExpandoObject>(seaTurtleTagAsJson);
                await seaTurtleTagService.DeleteSeaTurtleTag(seaTurtleTag.seaTurtleTagId.GetString());
            }

            var seaTurtleMorphometricService = new SeaTurtleMorphometricService(_organizationId, seaTurtleId);

            var seaTurtleMorphometrics = await seaTurtleMorphometricService.GetSeaTurtleMorphometrics();
            foreach (var seaTurtleMorphometricAsJson in seaTurtleMorphometrics)
            {
                dynamic seaTurtleMorphometric = JsonSerializer.Deserialize<ExpandoObject>(seaTurtleMorphometricAsJson);
                await seaTurtleMorphometricService.DeleteSeaTurtleMorphometric(seaTurtleMorphometric.seaTurtleMorphometricId.GetString());
            }

            return await _dataHelper.DeleteItemAsync(_pk, $"SEA_TURTLE#{seaTurtleId}");
        }
    }
}
