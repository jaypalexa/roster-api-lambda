using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using iTextSharp.text.pdf;
using Microsoft.IdentityModel.Tokens;
using RosterApiLambda.Dtos;
using RosterApiLambda.Handlers;
using RosterApiLambda.Helpers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace RosterApiLambda
{
    public class Function
    {
        public async Task<RosterResponse> FunctionHandler(RosterRequest request, ILambdaContext context)
        {
            LambdaLogger.Log($"REQUEST:  {request}");

            var response = new RosterResponse();

            if (request.resource == "/wake-up")
            {
                response.body.message = $"{request.resource} at: {DateTime.Now.ToUniversalTime()}";
                return response;
            }

            var jwt = request.headers["jwt"];
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            if (jwtSecurityTokenHandler.CanReadToken(jwt))
            {
                var decodedJwt = jwtSecurityTokenHandler.ReadJwtToken(jwt);
                var organizationId = Convert.ToString(decodedJwt.Payload["custom:organizationId"]);

                if (request.resource.StartsWith("/reports"))
                {
                    response.body = HandleReportRequest(organizationId, request);
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

        public string GetMasterReportFileName(string reportId)
        {
            var fileName = reportId switch
            {
                "MarineTurtleCaptiveFacilityQuarterlyReportForHatchlings" => "MASTER - Marine Turtle Captive Facility Quarterly Report For Hatchlings.pdf",
                "MarineTurtleCaptiveFacilityQuarterlyReportForWashbacks" => "MASTER - Marine Turtle Captive Facility Quarterly Report For Washbacks.pdf",
                "MarineTurtleHoldingFacilityQuarterlyReport" => "MASTER - Marine Turtle Holding Facility Quarterly Report.pdf",
                "TaggingDataForm" => "MASTER - Tagging Data form.pdf",

                _ => throw new NotImplementedException(),
            };

            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            return Path.Combine(basePath, "pdf", fileName);
        }

        public RosterResponseBody HandleReportRequest(string organizationId, RosterRequest request)
        {
            request.pathParameters.TryGetValue("reportId", out string reportId);

            var body = new RosterResponseBody();

            var masterReportFileName = GetMasterReportFileName(reportId);
            body.data = File.ReadAllBytes(masterReportFileName);
            return body;

            //var basePath = AppDomain.CurrentDomain.BaseDirectory;

            //var fieldsReportName = Path.Combine(basePath, "pdf", "test.pdf");
            ////var filledReportName = fieldsReportName.Replace("MASTER - ", "FILLED - ").Replace(".master.pdf", $" - {DateTime.Now:yyyyMMddHHmmss}.pdf");
            //var filledReportName = Path.Combine("/tmp", "test-filled.pdf");

            //var pdfReader = new PdfReader(fieldsReportName);
            //var fs = new FileStream(filledReportName, FileMode.Create);

            //var pdfStamper = new PdfStamper(pdfReader, fs, '\0', true);
            //var acroFields = pdfStamper.AcroFields;
            //acroFields.SetField("txtTurtlePermitNumber", "JPA PERMIT NUMBER");

            //// pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
            //pdfStamper.Close();
            //pdfReader.Close();

            //var bytes = File.ReadAllBytes(filledReportName);

            ////var base64 = Convert.ToBase64String(bytes); // converting to base64 seems to corrupt (?) and gets wrapped in double quotes?
            ////LambdaLogger.Log(base64);

            //body.data = bytes;

            //return body;
        }

        public async Task<RosterResponseBody> HandleDataRequest(string organizationId, RosterRequest request)
        {
            var body = new RosterResponseBody();

            body.data = request.resource switch
            {
                "/organizations/{organizationId}" => await OrganizationHandler.Handle(organizationId, request),
                "/sea-turtles" => await SeaTurtleHandler.Handle(organizationId, request),
                "/sea-turtles/{seaTurtleId}" => await SeaTurtleHandler.Handle(organizationId, request),
                "/sea-turtles/{seaTurtleId}/sea-turtle-tags" => await SeaTurtleTagHandler.Handle(organizationId, request),
                "/sea-turtles/{seaTurtleId}/sea-turtle-tags/{seaTurtleTagId}" => await SeaTurtleTagHandler.Handle(organizationId, request),
                "/sea-turtles/{seaTurtleId}/sea-turtle-morphometrics" => await SeaTurtleMorphometricHandler.Handle(organizationId, request),
                "/sea-turtles/{seaTurtleId}/sea-turtle-morphometrics/{seaTurtleMorphometricId}" => await SeaTurtleMorphometricHandler.Handle(organizationId, request),
                "/holding-tanks" => await HoldingTankHandler.Handle(organizationId, request),
                "/holding-tanks/{holdingTankId}" => await HoldingTankHandler.Handle(organizationId, request),
                "/holding-tanks/{holdingTankId}/holding-tank-measurements" => await HoldingTankMeasurementHandler.Handle(organizationId, request),
                "/holding-tanks/{holdingTankId}/holding-tank-measurements/{holdingTankMeasurementId}" => await HoldingTankMeasurementHandler.Handle(organizationId, request),
                "/hatchlings-events" => await HatchlingsEventHandler.Handle(organizationId, request),
                "/hatchlings-events/{hatchlingsEventId}" => await HatchlingsEventHandler.Handle(organizationId, request),
                "/washbacks-events" => await WashbacksEventHandler.Handle(organizationId, request),
                "/washbacks-events/{washbacksEventId}" => await WashbacksEventHandler.Handle(organizationId, request),
                _ => throw new ArgumentOutOfRangeException(ErrorHelper.InvalidResource(request.resource)),
            };

            return body;
        }
    }
}
