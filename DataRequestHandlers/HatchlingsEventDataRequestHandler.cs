using System;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.DataRequestHandlers.Interfaces;
using RosterApiLambda.Helpers;
using RosterApiLambda.Services;
using RosterApiLambda.Models;
using System.Text.Json;

namespace RosterApiLambda.DataRequestHandlers
{
    public class HatchlingsEventDataRequestHandler : IDataRequestHandler
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
