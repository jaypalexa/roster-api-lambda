using System;
using System.Threading.Tasks;
using RosterApiLambda.DataRequestHandlers.Interfaces;
using RosterApiLambda.Dtos;
using RosterApiLambda.Helpers;
using RosterApiLambda.Services;

namespace RosterApiLambda.DataRequestHandlers
{
    public class LastUpdateDataRequestHandler : IDataRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var lastUpdateService = new LastUpdateService(organizationId);

            return request.resource switch
            {
                "/last-update" => request.httpMethod switch
                {
                    "GET" => await lastUpdateService.GetLastUpdate(),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
