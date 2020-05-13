using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;

namespace RosterApiLambda.Services
{
    public class OrganizationService
    {
        private DataHelper _dataHelper { get; }
        private string _pk { get; }

        public OrganizationService(string organizationId)
        {
            _dataHelper = new DataHelper(organizationId);
            _pk = $"ORGANIZATION#{organizationId}";
        }

        public async Task<string> GetOrganization()
            => await _dataHelper.GetItemAsync(_pk, "ORGANIZATION");

        public async Task<PutItemResponse> SaveOrganization(string body)
            => await _dataHelper.PutItemAsync(_pk, "ORGANIZATION", body);
    }
}
