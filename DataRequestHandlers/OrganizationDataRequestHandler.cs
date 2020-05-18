using System;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.DataRequestHandlers.Interfaces;
using RosterApiLambda.Dtos;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.DataRequestHandlers
{
    public class OrganizationDataRequestHandler : IDataRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var organizationService = new OrganizationService(organizationId);

            return request.resource switch
            {
                "/organizations/{organizationId}" => request.httpMethod switch
                {
                    "GET" => await organizationService.GetOrganization(),
                    "PUT" => await organizationService.SaveOrganization(JsonSerializer.Deserialize<OrganizationModel>(request.body.GetRawText())),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
