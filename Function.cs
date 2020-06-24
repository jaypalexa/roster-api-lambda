using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.IdentityModel.Tokens;
using RosterApiLambda.DataRequestHandlers;
using RosterApiLambda.Dtos;
using RosterApiLambda.Helpers;
using RosterApiLambda.ReportRequestHandlers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RosterApiLambda
{
    public class Function
    {
        public async Task<RosterResponse> FunctionHandler(RosterRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"REQUEST:  {request}\r\n");

            var response = new RosterResponse();

            //if (request.resource == "/convert")
            //{
            //    await ConversionHelper.DoConversion("csvjson.turtle_tag.json");
            //    response.body.message = $"{request.resource} at: {DateTime.Now.ToUniversalTime()}";
            //    return response;
            //}

            var jwt = request.headers["jwt"];
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            if (jwtSecurityTokenHandler.CanReadToken(jwt))
            {
                var decodedJwt = jwtSecurityTokenHandler.ReadJwtToken(jwt);
                var organizationId = Convert.ToString(decodedJwt.Payload["custom:organizationId"]);

                if (request.resource.StartsWith("/reports"))
                {
                    response.body = await HandleReportRequest(organizationId, request);
                }
                else
                {
                    response.body = await HandleDataRequest(organizationId, request);
                }
            }
            else
            {
                throw new SecurityTokenValidationException("Unable to read JWT.");
            }

            return response;
        }

        public async Task<RosterResponseBody> HandleReportRequest(string organizationId, RosterRequest request)
        {
            request.pathParameters.TryGetValue("reportId", out string reportId);

            var body = new RosterResponseBody();

            body.data = reportId switch
            {
                /* FWC REPORTS (PDF) */
                "MarineTurtleCaptiveFacilityQuarterlyReportForHatchlings" => await MarineTurtleCaptiveFacilityQuarterlyReportRequestHandler.Handle(organizationId, request),
                "MarineTurtleCaptiveFacilityQuarterlyReportForWashbacks" => await MarineTurtleCaptiveFacilityQuarterlyReportRequestHandler.Handle(organizationId, request),
                "MarineTurtleHoldingFacilityQuarterlyReport" => await MarineTurtleHoldingFacilityQuarterlyReportRequestHandler.Handle(organizationId, request),
                "TaggingDataForm" => await TaggingDataFormReportRequestHandler.Handle(organizationId, request),

                /* OTHER REPORTS (HTML) */
                "HatchlingsAndWashbacksByCountyReport" => await HatchlingsAndWashbacksByCountyReportRequestHandler.Handle(organizationId, request),
                "TurtleInjuryReport" => await TurtleInjuryReportRequestHandler.Handle(organizationId, request),
                "TurtleTagReport" => await TurtleTagReportRequestHandler.Handle(organizationId, request),

                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidReportId(reportId)),
            };

            return body;
        }

        public async Task<RosterResponseBody> HandleDataRequest(string organizationId, RosterRequest request)
        {
            var body = new RosterResponseBody();

            body.data = request.resource switch
            {
                "/last-update" => await LastUpdateDataRequestHandler.Handle(organizationId, request),
                "/organizations/{organizationId}" => await OrganizationDataRequestHandler.Handle(organizationId, request),
                "/sea-turtle-list-items" => await SeaTurtleDataRequestHandler.Handle(organizationId, request),
                "/sea-turtles" => await SeaTurtleDataRequestHandler.Handle(organizationId, request),
                "/sea-turtles/{seaTurtleId}" => await SeaTurtleDataRequestHandler.Handle(organizationId, request),
                "/sea-turtles/{seaTurtleId}/sea-turtle-tags" => await SeaTurtleTagDataRequestHandler.Handle(organizationId, request),
                "/sea-turtles/{seaTurtleId}/sea-turtle-tags/{seaTurtleTagId}" => await SeaTurtleTagDataRequestHandler.Handle(organizationId, request),
                "/sea-turtles/{seaTurtleId}/sea-turtle-morphometrics" => await SeaTurtleMorphometricDataRequestHandler.Handle(organizationId, request),
                "/sea-turtles/{seaTurtleId}/sea-turtle-morphometrics/{seaTurtleMorphometricId}" => await SeaTurtleMorphometricDataRequestHandler.Handle(organizationId, request),
                "/holding-tanks" => await HoldingTankDataRequestHandler.Handle(organizationId, request),
                "/holding-tanks/{holdingTankId}" => await HoldingTankDataRequestHandler.Handle(organizationId, request),
                "/holding-tanks/{holdingTankId}/holding-tank-measurements" => await HoldingTankMeasurementDataRequestHandler.Handle(organizationId, request),
                "/holding-tanks/{holdingTankId}/holding-tank-measurements/{holdingTankMeasurementId}" => await HoldingTankMeasurementDataRequestHandler.Handle(organizationId, request),
                "/hatchlings-events" => await HatchlingsEventDataRequestHandler.Handle(organizationId, request),
                "/hatchlings-events/{hatchlingsEventId}" => await HatchlingsEventDataRequestHandler.Handle(organizationId, request),
                "/washbacks-events" => await WashbacksEventDataRequestHandler.Handle(organizationId, request),
                "/washbacks-events/{washbacksEventId}" => await WashbacksEventDataRequestHandler.Handle(organizationId, request),
                "/log-entries" => await LogEntryDataRequestHandler.Handle(organizationId, request),
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };

            return body;
        }
    }
}
