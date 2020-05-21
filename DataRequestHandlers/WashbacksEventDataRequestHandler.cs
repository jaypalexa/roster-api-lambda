using System;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.DataRequestHandlers
{
    public class WashbacksEventDataRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            request.pathParameters.TryGetValue("washbacksEventId", out string washbacksEventId);

            var washbacksEventService = new WashbacksEventService(organizationId);

            return request.resource switch
            {
                "/washbacks-events" => request.httpMethod switch
                {
                    "GET" => await washbacksEventService.GetWashbacksEvents(),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                "/washbacks-events/{washbacksEventId}" => request.httpMethod switch
                {
                    "GET" => await washbacksEventService.GetWashbacksEvent(washbacksEventId),
                    "PUT" => await washbacksEventService.SaveWashbacksEvent(washbacksEventId, JsonSerializer.Deserialize<WashbacksEventModel>(request.body.GetRawText())),
                    "DELETE" => await washbacksEventService.DeleteWashbacksEvent(washbacksEventId),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
