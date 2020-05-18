using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;

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

        public async Task<OrganizationModel> GetOrganization()
            => await _dataHelper.GetItemAsync<OrganizationModel>(_pk, "ORGANIZATION");

        public async Task<PutItemResponse> SaveOrganization(OrganizationModel organization)
        {
            return await _dataHelper.PutItemAsync(_pk, "ORGANIZATION", organization);
        }

        public async Task<int> GetLastUpdate()
        {
            var lastUpdateModel = await _dataHelper.GetItemAsync<LastUpdateModel>("LAST_UPDATE", "LAST_UPDATE");
            return lastUpdateModel.lastUpdate;
        }
    }
}
