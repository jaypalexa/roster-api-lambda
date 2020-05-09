using System;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Handlers.Interfaces;
using RosterApiLambda.Helpers;
using RosterApiLambda.Services;

namespace RosterApiLambda.Handlers
{
    public class SeaTurtleTagHandler : IHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            request.pathParameters.TryGetValue("seaTurtleId", out string seaTurtleId);
            request.pathParameters.TryGetValue("seaTurtleTagId", out string seaTurtleTagId);

            var seaTurtleTagService = new SeaTurtleTagService(organizationId, seaTurtleId);

            return request.resource switch
            {
                "/sea-turtles/{seaTurtleId}/sea-turtle-tags" => request.httpMethod switch
                {
                    "GET" => await seaTurtleTagService.GetSeaTurtleTags(),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                "/sea-turtles/{seaTurtleId}/sea-turtle-tags/{seaTurtleTagId}" => request.httpMethod switch
                {
                    "GET" => await seaTurtleTagService.GetSeaTurtleTag(seaTurtleTagId),
                    "PUT" => await seaTurtleTagService.SaveSeaTurtleTag(seaTurtleTagId, request.body),
                    "DELETE" => await seaTurtleTagService.DeleteSeaTurtleTag(seaTurtleTagId),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
