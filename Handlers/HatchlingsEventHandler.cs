using System;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Handlers.Interfaces;
using RosterApiLambda.Helpers;
using RosterApiLambda.Services;

namespace RosterApiLambda.Handlers
{
    public class HatchlingsEventHandler : IHandler
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
                    "PUT" => await hatchlingsEventService.SaveHatchlingsEvent(hatchlingsEventId, request.body),
                    "DELETE" => await hatchlingsEventService.DeleteHatchlingsEvent(hatchlingsEventId),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
