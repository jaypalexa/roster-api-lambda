using System;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Handlers.Interfaces;
using RosterApiLambda.Helpers;
using RosterApiLambda.Services;

namespace RosterApiLambda.Handlers
{
    public class SeaTurtleMorphometricHandler : IHandler
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
                    "PUT" => await seaTurtleMorphometricService.SaveSeaTurtleMorphometric(seaTurtleMorphometricId, request.body),
                    "DELETE" => await seaTurtleMorphometricService.DeleteSeaTurtleMorphometric(seaTurtleMorphometricId),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
