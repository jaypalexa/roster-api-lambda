﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using RosterApiLambda.Dtos;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.DataRequestHandlers
{
    public static class HoldingTankMeasurementDataRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            request.pathParameters.TryGetValue("holdingTankId", out string holdingTankId);
            request.pathParameters.TryGetValue("holdingTankMeasurementId", out string holdingTankMeasurementId);

            var holdingTankMeasurementService = new HoldingTankMeasurementService(organizationId, holdingTankId);

            return request.resource switch
            {
                "/holding-tanks/{holdingTankId}/holding-tank-measurements" => request.httpMethod switch
                {
                    "GET" => await holdingTankMeasurementService.GetHoldingTankMeasurements(),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                "/holding-tanks/{holdingTankId}/holding-tank-measurements/{holdingTankMeasurementId}" => request.httpMethod switch
                {
                    "GET" => await holdingTankMeasurementService.GetHoldingTankMeasurement(holdingTankMeasurementId),
                    "PUT" => await holdingTankMeasurementService.SaveHoldingTankMeasurement(holdingTankMeasurementId, JsonSerializer.Deserialize<HoldingTankMeasurementModel>(request.body.GetRawText())),
                    "DELETE" => await holdingTankMeasurementService.DeleteHoldingTankMeasurement(holdingTankMeasurementId),
                    _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidHttpMethodForResource(request.httpMethod, request.resource)),
                },
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };
        }
    }
}
