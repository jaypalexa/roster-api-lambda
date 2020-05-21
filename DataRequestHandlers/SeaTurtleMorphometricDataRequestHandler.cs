using System;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.DataRequestHandlers
{
    public class SeaTurtleMorphometricDataRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            request.pathParameters.TryGetValue("seaTurtleId", out string seaTurtleId);
            request.pathParameters.TryGetValue("seaTurtleMorphometricId", out string seaTurtleMorphometricId);

            var seaTurtleMorphometricService = new SeaTurtleMorphometricService(organizationId, seaTurtleId);

            return request.resource switch
            {
                "/sea-turtles/{seaTurtleId}/sea-turtle-morphometrics" => request.httpMethod switch
                {
                    "GET" => await seaTurtleMorphometricService.GetSeaTurtleMorphometrics(),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                "/sea-turtles/{seaTurtleId}/sea-turtle-morphometrics/{seaTurtleMorphometricId}" => request.httpMethod switch
                {
                    "GET" => await seaTurtleMorphometricService.GetSeaTurtleMorphometric(seaTurtleMorphometricId),
                    "PUT" => await seaTurtleMorphometricService.SaveSeaTurtleMorphometric(seaTurtleMorphometricId, JsonSerializer.Deserialize<SeaTurtleMorphometricModel>(request.body.GetRawText())),
                    "DELETE" => await seaTurtleMorphometricService.DeleteSeaTurtleMorphometric(seaTurtleMorphometricId),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
