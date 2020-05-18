using System.Threading.Tasks;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;

namespace RosterApiLambda.Services
{
    public class LastUpdateService
    {
        private DataHelper _dataHelper { get; }

        public LastUpdateService(string organizationId)
        {
            _dataHelper = new DataHelper(organizationId);
        }

        public async Task<int> GetLastUpdate()
        {
            var lastUpdateModel = await _dataHelper.GetItemAsync<LastUpdateModel>("LAST_UPDATE", "LAST_UPDATE");
            return lastUpdateModel.lastUpdate;
        }
    }
}
