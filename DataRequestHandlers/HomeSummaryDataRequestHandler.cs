using RosterApiLambda.Dtos;
using RosterApiLambda.Helpers;
using RosterApiLambda.Services;
using System;
using System.Threading.Tasks;

namespace RosterApiLambda.DataRequestHandlers
{
    public static class HomeSummaryDataRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var homeSummaryService = new HomeSummaryService();

            return request.resource switch
            {
                "/home-summary" => request.httpMethod switch
                {
                    "GET" => await homeSummaryService.GetHomeSummary(organizationId),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
