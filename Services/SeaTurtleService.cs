using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;

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

        public async Task<IEnumerable<SeaTurtleListItemModel>> GetSeaTurtleListItems()
        {
            var seaTurtles = await GetSeaTurtles();

            var seaTurtleListItems = seaTurtles.Select(x => JsonSerializer.Deserialize<SeaTurtleListItemModel>(JsonSerializer.Serialize(x)));

            return seaTurtleListItems;
        }

        public async Task<List<SeaTurtleModel>> GetSeaTurtles()
            => await _dataHelper.QueryAsync<SeaTurtleModel>(_pk, "SEA_TURTLE#");

        public async Task<SeaTurtleModel> GetSeaTurtle(string seaTurtleId)
            => await _dataHelper.GetItemAsync<SeaTurtleModel>(_pk, $"SEA_TURTLE#{seaTurtleId}");

        public async Task<PutItemResponse> SaveSeaTurtle(string seaTurtleId, SeaTurtleModel seaTurtle)
            => await _dataHelper.PutItemAsync(_pk, $"SEA_TURTLE#{seaTurtleId}", seaTurtle);

        public async Task<DeleteItemResponse> DeleteSeaTurtle(string seaTurtleId)
        {
            var seaTurtleTagService = new SeaTurtleTagService(_organizationId, seaTurtleId);

            var seaTurtleTags = await seaTurtleTagService.GetSeaTurtleTags();
            foreach (var seaTurtleTag in seaTurtleTags)
            {
                await seaTurtleTagService.DeleteSeaTurtleTag(seaTurtleTag.seaTurtleTagId);
            }

            var seaTurtleMorphometricService = new SeaTurtleMorphometricService(_organizationId, seaTurtleId);

            var seaTurtleMorphometrics = await seaTurtleMorphometricService.GetSeaTurtleMorphometrics();
            foreach (var seaTurtleMorphometric in seaTurtleMorphometrics)
            {
                await seaTurtleMorphometricService.DeleteSeaTurtleMorphometric(seaTurtleMorphometric.seaTurtleMorphometricId);
            }

            return await _dataHelper.DeleteItemAsync(_pk, $"SEA_TURTLE#{seaTurtleId}");
        }
    }
}
