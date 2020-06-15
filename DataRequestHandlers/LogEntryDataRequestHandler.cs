using System;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.DataRequestHandlers
{
    public static class LogEntryDataRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            request.pathParameters.TryGetValue("logEntryId", out string logEntryId);

            var logEntryService = new LogEntryService(organizationId);

            return request.resource switch
            {
                "/log-entries" => request.httpMethod switch
                {
                    "GET" => await logEntryService.GetLogEntries(),
                    "PUT" => await logEntryService.SaveLogEntry(JsonSerializer.Deserialize<LogEntryModel>(request.body.GetRawText())),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
