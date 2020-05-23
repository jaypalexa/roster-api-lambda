using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using RosterApiLambda.Dtos;
using RosterApiLambda.Dtos.ReportOptions;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.ReportRequestHandlers
{
    public class MarineTurtleCaptiveFacilityQuarterlyReportRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            var reportOptions = JsonSerializer.Deserialize<MarineTurtleCaptiveFacilityQuarterlyReportOptionsDto>(request.body.GetRawText());

            var fileTimestamp = $"{DateTime.Now:yyyyMMddHHmmss} UTC";

            var baseMasterReportFileName = $"MASTER - Marine Turtle Captive Facility Quarterly Report For {reportOptions.subjectType}.pdf";
            var basePath = AppDomain.CurrentDomain.BaseDirectory;

            var masterReportFileName = Path.Combine(basePath, "pdf", baseMasterReportFileName);
            var filledReportFileName = Path.Combine("/tmp", baseMasterReportFileName.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {fileTimestamp}.pdf"));

            var organizationService = new OrganizationService(organizationId);
            var organization = await organizationService.GetOrganization();
            var organizationAndPermitNumber = $"{organization.organizationName} - {organization.permitNumber}";

            //var seaTurtleService = new SeaTurtleService(organizationId);
            //var seaTurtle = await seaTurtleService.GetSeaTurtle(reportOptions.seaTurtleId);

            //var seaTurtleTagService = new SeaTurtleTagService(organizationId, reportOptions.seaTurtleId);
            //var seaTurtleTags = await seaTurtleTagService.GetSeaTurtleTags();

            //var seaTurtleMorphometricService = new SeaTurtleMorphometricService(organizationId, reportOptions.seaTurtleId);
            //var seaTurtleMorphometrics = await seaTurtleMorphometricService.GetSeaTurtleMorphometrics();

            //var nonPitTags = seaTurtleTags.Where(x => x.tagType != "PIT" && !string.IsNullOrWhiteSpace(x.tagNumber));

            //var flipperTagLeftFront = string.Join(", ", nonPitTags.Where(x => x.location == "LFF").Select(x => x.tagNumber));
            //var flipperTagRightFront = string.Join(", ", nonPitTags.Where(x => x.location == "RFF").Select(x => x.tagNumber));
            //var flipperTagLeftRear = string.Join(", ", nonPitTags.Where(x => x.location == "LRF").Select(x => x.tagNumber));
            //var flipperTagRightRear = string.Join(", ", nonPitTags.Where(x => x.location == "RRF").Select(x => x.tagNumber));

            //var pitTags = seaTurtleTags.Where(x => x.tagType == "PIT");
            //var pitTagNumber = string.Join(", ", pitTags.Where(x => !string.IsNullOrWhiteSpace(x.tagNumber)).Select(x => x.tagNumber));
            //var pitTagLocation = string.Join(", ", pitTags.Where(x => !string.IsNullOrWhiteSpace(x.location)).Select(x => x.location));

            //----------------------------------------------------------------------------------------------------

            var pdfReader = new PdfReader(masterReportFileName);
            pdfReader.RemoveUsageRights();

            var fs = new FileStream(filledReportFileName, FileMode.Create);
            var pdfStamper = new PdfStamper(pdfReader, fs, '\0', false);

            var info = pdfReader.Info;
            info["Title"] = baseMasterReportFileName.Replace("MASTER - ", "").Replace(".pdf", $" - {fileTimestamp}.pdf");
            pdfStamper.MoreInfo = info;

            var acroFields = pdfStamper.AcroFields;

            acroFields.SetField("txtOrganizationAndPermitNumber", organizationAndPermitNumber);

            //acroFields.SetField("txtSID", reportOptions.printSidOnForm ? $"SID:  {seaTurtle.sidNumber}" : string.Empty);
            //acroFields.SetField("txtSpecies", seaTurtle.species);

            //if (!string.IsNullOrEmpty(seaTurtle.dateRelinquished))
            //{
            //    acroFields.SetField("txtDateReleasedDay", seaTurtle.dateRelinquished.Substring(8, 2));
            //    acroFields.SetField("txtDateReleasedMonth", seaTurtle.dateRelinquished.Substring(5, 2));
            //    acroFields.SetField("txtDateReleasedYear", seaTurtle.dateRelinquished.Substring(0, 4));
            //}

            //acroFields.SetField("txtFlipperTagLeftFront", flipperTagLeftFront);

            // =============================================================================

            pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
            pdfStamper.Close();
            pdfReader.Close();

            var bytes = await File.ReadAllBytesAsync(filledReportFileName);

            return bytes;
        }
    }
}
