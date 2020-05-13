using System;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.DataRequestHandlers.Interfaces;
using RosterApiLambda.Helpers;
using RosterApiLambda.Services;

namespace RosterApiLambda.DataRequestHandlers
{
    public class HoldingTankDataRequestHandler : IDataRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            request.pathParameters.TryGetValue("holdingTankId", out string holdingTankId);

            var holdingTankService = new HoldingTankService(organizationId);

            return request.resource switch
            {
                "/holding-tanks" => request.httpMethod switch
                {
                    "GET" => await holdingTankService.GetHoldingTanks(),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                "/holding-tanks/{holdingTankId}" => request.httpMethod switch
                {
                    "GET" => await holdingTankService.GetHoldingTank(holdingTankId),
                    "PUT" => await holdingTankService.SaveHoldingTank(holdingTankId, request.body),
                    "DELETE" => await holdingTankService.DeleteHoldingTank(holdingTankId),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
