using System;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.DataRequestHandlers
{
    public static class HatchlingsEventDataRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            request.pathParameters.TryGetValue("hatchlingsEventId", out string hatchlingsEventId);

            var hatchlingsEventService = new HatchlingsEventService(organizationId);

            return request.resource switch
            {
                "/hatchlings-events" => request.httpMethod switch
                {
                    "GET" => await hatchlingsEventService.GetHatchlingsEvents(),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                "/hatchlings-events/{hatchlingsEventId}" => request.httpMethod switch
                {
                    "GET" => await hatchlingsEventService.GetHatchlingsEvent(hatchlingsEventId),
                    "PUT" => await hatchlingsEventService.SaveHatchlingsEvent(hatchlingsEventId, JsonSerializer.Deserialize<HatchlingsEventModel>(request.body.GetRawText())),
                    "DELETE" => await hatchlingsEventService.DeleteHatchlingsEvent(hatchlingsEventId),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
