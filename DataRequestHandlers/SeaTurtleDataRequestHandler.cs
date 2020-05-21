using System;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.DataRequestHandlers
{
    public class SeaTurtleDataRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            request.pathParameters.TryGetValue("seaTurtleId", out string seaTurtleId);

            var seaTurtleService = new SeaTurtleService(organizationId);

            return request.resource switch
            {
                "/sea-turtle-list-items" => request.httpMethod switch
                {
                    "GET" => await seaTurtleService.GetSeaTurtleListItems(),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                "/sea-turtles" => request.httpMethod switch
                {
                    "GET" => await seaTurtleService.GetSeaTurtles(),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                "/sea-turtles/{seaTurtleId}" => request.httpMethod switch
                {
                    "GET" => await seaTurtleService.GetSeaTurtle(seaTurtleId),
                    "PUT" => await seaTurtleService.SaveSeaTurtle(seaTurtleId, JsonSerializer.Deserialize<SeaTurtleModel>(request.body.GetRawText())),
                    "DELETE" => await seaTurtleService.DeleteSeaTurtle(seaTurtleId),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
