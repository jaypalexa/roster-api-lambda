using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Asn1;
using RosterApiLambda.Dtos;
using RosterApiLambda.Extensions;
using RosterApiLambda.Models;
using RosterApiLambda.ReportRequestHandlers.Interfaces;
using RosterApiLambda.Services;

namespace RosterApiLambda.ReportRequestHandlers
{
    public class TaggingDataFormReportRequestHandler : IReportRequestHandler
    {
        public static async Task<object> Handle(string organizationId, string reportId, RosterRequest request)
        {
            /*
                "MarineTurtleCaptiveFacilityQuarterlyReportForHatchlings" => "MASTER - Marine Turtle Captive Facility Quarterly Report For Hatchlings.pdf",
                "MarineTurtleCaptiveFacilityQuarterlyReportForWashbacks" => "MASTER - Marine Turtle Captive Facility Quarterly Report For Washbacks.pdf",
                "MarineTurtleHoldingFacilityQuarterlyReport" => "MASTER - Marine Turtle Holding Facility Quarterly Report.pdf",
                "TaggingDataForm" => "MASTER - Tagging Data form.pdf",
            */

            var baseMasterReportFileName = "MASTER - Tagging Data form.pdf";
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            var masterReportFileName = Path.Combine(basePath, "pdf", baseMasterReportFileName);
            var filledReportFileName = Path.Combine("/tmp", baseMasterReportFileName.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {DateTime.Now:yyyyMMddHHmmss}.pdf"));

            var organizationService = new OrganizationService(organizationId);
            string organizationAsJson = await organizationService.GetOrganization();
            var organization = JsonSerializer.Deserialize<OrganizationModel>(organizationAsJson);
            var organizationInformation = $"{organization.organizationName} - {organization.phone} - {organization.emailAddress}";

            IDictionary<string, object> requestBody = JsonSerializer.Deserialize<ExpandoObject>(request.body);
            var seaTurtleId = requestBody.GetString("seaTurtleId");
            var populateFacilityField = requestBody.GetBoolean("populateFacilityField");

            var seaTurtleService = new SeaTurtleService(organizationId);
            string seaTurtleAsJson = await seaTurtleService.GetSeaTurtle(seaTurtleId);
            var seaTurtle = JsonSerializer.Deserialize<SeaTurtleModel>(seaTurtleAsJson);

            var pdfReader = new PdfReader(masterReportFileName);
            pdfReader.RemoveUsageRights();

            var fs = new FileStream(filledReportFileName, FileMode.Create);

            var pdfStamper = new PdfStamper(pdfReader, fs, '\0', false);
            var acroFields = pdfStamper.AcroFields;

            acroFields.SetField("txtSID", $"SID:  {seaTurtle.sidNumber}");
            acroFields.SetField("txtSpecies", seaTurtle.species);

            // YYYY-MM-DD
            // 0123456789
            string dateCaptured = seaTurtle.dateCaptured ?? seaTurtle.dateAcquired;
            string dateRelinquished = seaTurtle.dateRelinquished;

            if (!string.IsNullOrEmpty(dateCaptured))
            {
                acroFields.SetField("txtDateCapturedDay", dateCaptured.Substring(8, 2));
                acroFields.SetField("txtDateCapturedMonth", dateCaptured.Substring(5, 2));
                acroFields.SetField("txtDateCapturedYear", dateCaptured.Substring(0, 4));
            }

            if (!string.IsNullOrEmpty(dateRelinquished))
            {
                acroFields.SetField("txtDateReleasedDay", dateRelinquished.Substring(8, 2));
                acroFields.SetField("txtDateReleasedMonth", dateRelinquished.Substring(5, 2));
                acroFields.SetField("txtDateReleasedYear", dateRelinquished.Substring(0, 4));
            }

            if (seaTurtle.wasCarryingTagsWhenEnc)
            {
                acroFields.SetField("radTurtleCarryingTags", "Yes");

                if (seaTurtle.recaptureType == "S")
                {
                    acroFields.SetField("radRecapture", "1");
                }
                else if (seaTurtle.recaptureType == "D")
                {
                    acroFields.SetField("radRecapture", "2");
                }
            }
            else
            {
                acroFields.SetField("radTurtleCarryingTags", "No");
            }

            acroFields.SetField("txtTagReturnAddress", seaTurtle.tagReturnAddress);

            acroFields.SetField("txtOrganizationInformation", organizationInformation);

            switch (seaTurtle.captureProjectType)
            {
                case "N":
                    acroFields.SetField("radProjectType", "NestingBeach");
                    switch (seaTurtle.didTurtleNest)
                    {
                        case "Y":
                            acroFields.SetField("radDidTurtleNest", "Yes");
                            break;
                        case "N":
                            acroFields.SetField("radDidTurtleNest", "No");
                            break;
                        case "U":
                            acroFields.SetField("radDidTurtleNest", "Undetermined");
                            break;
                        default:
                            break;
                    }
                    break;
                case "T":
                    acroFields.SetField("radProjectType", "TangleNet");
                    break;
                case "P":
                    acroFields.SetField("radProjectType", "PoundNet");
                    break;
                case "H":
                    acroFields.SetField("radProjectType", "HandCatch");
                    break;
                case "S":
                    acroFields.SetField("radProjectType", "Stranding");
                    break;
                case "O":
                    acroFields.SetField("radProjectType", "Other");
                    acroFields.SetField("txtProjectTypeOther", seaTurtle.captureProjectOther);
                    break;
                default:
                    break;
            }

            if (populateFacilityField)
            {
                acroFields.SetField("txtFacility", organizationInformation);
            }

            var acquiredCounty = !string.IsNullOrEmpty(seaTurtle.acquiredCounty) ? $"; County: {seaTurtle.acquiredCounty}" : "";
            var acquiredLatitude = !string.IsNullOrEmpty(seaTurtle.acquiredLatitude) ? $"; Latitude: {seaTurtle.acquiredLatitude}" : "";
            var acquiredLongitude = !string.IsNullOrEmpty(seaTurtle.acquiredLongitude) ? $"; Longitude: {seaTurtle.acquiredLongitude}" : "";
            var captureLocation = $"{seaTurtle.acquiredFrom}{acquiredCounty}{acquiredLatitude}{acquiredLongitude}".TrimStart(' ', ';');
            acroFields.SetField("txtCaptureLocation", captureLocation);

            pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
            pdfStamper.Close();
            pdfReader.Close();

            var bytes = await File.ReadAllBytesAsync(filledReportFileName);

            return bytes;
        }
    }
}
