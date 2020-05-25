using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using AutoMapper;
using iTextSharp.text.pdf;
using RosterApiLambda.Dtos;
using RosterApiLambda.Dtos.ReportOptions;
using RosterApiLambda.Extensions;
using RosterApiLambda.Helpers;
using RosterApiLambda.Models;
using RosterApiLambda.Services;

namespace RosterApiLambda.ReportRequestHandlers
{
    public class MarineTurtleHoldingFacilityQuarterlyReportRequestHandler
    {
        public static async Task<object> Handle(string organizationId, RosterRequest request)
        {
            const int PAGE_1_LINES_PER_PAGE = 8;
            const int PAGE_2_LINES_PER_PAGE = 22;

            var filledReportFileNames = new List<string>();
            var fileTimestamp = $"{DateTime.Now:yyyyMMddHHmmss} UTC";
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            PdfReader pdfReader;

            var reportOptions = JsonSerializer.Deserialize<MarineTurtleHoldingFacilityQuarterlyReportOptionsDto>(request.body.GetRawText());
            reportOptions.dateFrom ??= "0000-00-00";
            reportOptions.dateThru ??= "9999-99-99";

            var organizationService = new OrganizationService(organizationId);
            var organization = await organizationService.GetOrganization();
            var organizationAndPermitNumber = $"{organization.organizationName} - {organization.permitNumber}";

            string monthsAndYearOfReport;
            var dateFrom = ReportHelper.ToDate(reportOptions.dateFrom);
            var dateThru = ReportHelper.ToDate(reportOptions.dateThru);

            if (dateFrom.Year == dateThru.Year)
            {
                monthsAndYearOfReport = $"{dateFrom:dd} {dateFrom:MMMM} - {dateThru:dd} {dateThru:MMMM} {dateThru.Year}";
            }
            else
            {
                monthsAndYearOfReport = $"{dateFrom:dd} {dateFrom:MMMM} {dateFrom.Year} - {dateThru:dd} {dateThru:MMMM} {dateThru.Year}";
            }

            var seaTurtleService = new SeaTurtleService(organizationId);
            var seaTurtles = (await seaTurtleService.GetSeaTurtles())
                .Where(x => !string.IsNullOrEmpty(x.dateAcquired) && x.dateAcquired.CompareTo(reportOptions.dateThru) <= 0)
                .Where(x => string.IsNullOrEmpty(x.dateRelinquished) || (!string.IsNullOrEmpty(x.dateRelinquished) && reportOptions.dateFrom.CompareTo(x.dateRelinquished) <= 0))
                .OrderBy(x => x.sidNumber)
                .ThenBy(x => x.dateAcquired)
                .ThenBy(x => x.seaTurtleName);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<SeaTurtleModel, HoldingFacilityReportItem>();
            });
            var mapper = new Mapper(config);

            var items = new List<HoldingFacilityReportItem>();

            foreach (var seaTurtle in seaTurtles)
            {
                //'----------------------------------------------------------------
                //'-- kludge to account for all the data we want to cram into the 
                //'-- status/tag number line...ugh...
                //'----------------------------------------------------------------
                var reportTagNumberFieldData = await GetReportTagNumberFieldData(organizationId, seaTurtle, reportOptions);
                var lines = ReportHelper.WrapLine(reportTagNumberFieldData, 92);
                for (int i = 0; i < lines.Length; i++)
                {
                    var item = new HoldingFacilityReportItem();
                    if (i == 0)
                    {
                        item = mapper.Map<HoldingFacilityReportItem>(seaTurtle);
                    }
                    item.reportTagNumberFieldData = lines[i];
                    items.Add(item);
                }
            }

            //-- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] -- [PAGE 1] --

            var baseMasterReportFileName_Page1 = $"MASTER - Marine Turtle Holding Facility Quarterly Report Page 1.pdf";

            var masterReportFileName_Page1 = Path.Combine(basePath, "pdf", baseMasterReportFileName_Page1);
            var filledReportFileName_Page1 = Path.Combine("/tmp", baseMasterReportFileName_Page1.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {fileTimestamp}.pdf"));
            filledReportFileNames.Add(filledReportFileName_Page1);

            pdfReader = new PdfReader(masterReportFileName_Page1);
            pdfReader.RemoveUsageRights();

            using (var fs = new FileStream(filledReportFileName_Page1, FileMode.Create))
            {
                var pdfStamper = new PdfStamper(pdfReader, fs, '\0', false);

                var info = pdfReader.Info;
                info["Title"] = baseMasterReportFileName_Page1.Replace("MASTER - ", "").Replace(".pdf", $" - {fileTimestamp}.pdf");
                pdfStamper.MoreInfo = info;

                var acroFields = pdfStamper.AcroFields;

                acroFields.SetField("txtOrganizationAndPermitNumber", organizationAndPermitNumber);
                acroFields.SetField("txtMonthsAndYearOfReport", monthsAndYearOfReport);

                var pageOneItems = items.Take(PAGE_1_LINES_PER_PAGE).ToList();
                for (int i = 0; i < pageOneItems.Count(); i++)
                {
                    var item = pageOneItems[i];
                    FillSectionOneRow(acroFields, (i + 1).ToString().PadLeft(2, '0'), item, reportOptions);
                }

                pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
                pdfStamper.Close();
            }
            pdfReader.Close();

            //-- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] -- [PAGE 2] --
            var page2Items = items.Skip(PAGE_1_LINES_PER_PAGE).ToList().ChunkBy(PAGE_2_LINES_PER_PAGE);

            for (int chunkIndex = 0; chunkIndex < page2Items.Count(); chunkIndex++)
            {
                var baseMasterReportFileName_Page2 = $"MASTER - Marine Turtle Holding Facility Quarterly Report Page 2.pdf";

                var masterReportFileName_Page2 = Path.Combine(basePath, "pdf", baseMasterReportFileName_Page2);
                var filledReportFileName_Page2 = Path.Combine("/tmp", baseMasterReportFileName_Page2.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {fileTimestamp} - {chunkIndex.ToString().PadLeft(2, '0')}.pdf"));
                filledReportFileNames.Add(filledReportFileName_Page2);

                pdfReader = new PdfReader(masterReportFileName_Page2);
                pdfReader.RemoveUsageRights();

                using (var fs = new FileStream(filledReportFileName_Page2, FileMode.Create))
                {
                    var pdfStamper = new PdfStamper(pdfReader, fs, '\0', false);

                    var info = pdfReader.Info;
                    info["Title"] = baseMasterReportFileName_Page2.Replace("MASTER - ", "").Replace(".pdf", $" - {fileTimestamp}.pdf");
                    pdfStamper.MoreInfo = info;

                    var acroFields = pdfStamper.AcroFields;

                    acroFields.SetField("txtOrganizationAndPermitNumber", organizationAndPermitNumber);
                    acroFields.SetField("txtMonthsAndYearOfReport", monthsAndYearOfReport);

                    for (int i = 0; i < page2Items[chunkIndex].Count(); i++)
                    {
                        var item = page2Items[chunkIndex][i];
                        FillSectionOneRow(acroFields, (i + 1 + PAGE_1_LINES_PER_PAGE).ToString().PadLeft(2, '0'), item, reportOptions);
                    }

                    pdfStamper.FormFlattening = true; // 'true' to make the PDF read-only
                    pdfStamper.Close();
                }
                pdfReader.Close();
            }

            //-- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] -- [PAGE 3] --

            // =========================================================================================================================

            var masterReportFileName_Final = $"MASTER - Marine Turtle Holding Facility Quarterly Report.pdf";
            var filledReportFileName_Final = Path.Combine("/tmp", masterReportFileName_Final.Replace("MASTER - ", "FILLED - ").Replace(".pdf", $" - {fileTimestamp}.pdf"));

            ReportHelper.ConcatenatePdfFiles(filledReportFileNames, filledReportFileName_Final);

            var bytes = await File.ReadAllBytesAsync(filledReportFileName_Final);

            return bytes;
        }

        private static async Task<string> GetReportTagNumberFieldData(string organizationId, SeaTurtleModel seaTurtle, MarineTurtleHoldingFacilityQuarterlyReportOptionsDto reportOptions)
        {
            var sb = new StringBuilder();

            var dateFrom = ReportHelper.ToDate(reportOptions.dateFrom);
            var dateThru = ReportHelper.ToDate(reportOptions.dateThru);

            //----------------------------------------------------------------
            //-- display DATE ACQUIRED -only- when requested 
            //-- and when report date range is for one quarter or less
            //----------------------------------------------------------------
            //-- if the DATE ACQUIRED is within the date range of the report, 
            //-- then do display the ACQUIRED FROM information
            //----------------------------------------------------------------
            if (reportOptions.includeAcquiredFrom && !string.IsNullOrEmpty(seaTurtle.dateAcquired) && ((dateThru.Date - dateFrom.Date).Days <= 95))
            {
                if ((reportOptions.dateFrom.CompareTo(seaTurtle.dateAcquired) <= 0) && (seaTurtle.dateAcquired.CompareTo(reportOptions.dateThru) <= 0))
                {
                    if (!string.IsNullOrEmpty(seaTurtle.acquiredFrom))
                    {
                        sb.Append($"Acq. from: {seaTurtle.acquiredFrom}; ");
                    }
                }
            }

            var seaTurtleTagService = new SeaTurtleTagService(organizationId, seaTurtle.seaTurtleId);
            var seaTurtleTags = await seaTurtleTagService.GetSeaTurtleTags();

            if (seaTurtleTags.Count > 0)
            {
                sb.Append($"Tags: {string.Join(", ", seaTurtleTags.Select(x => x.tagNumber))}; ");
            }

            if (reportOptions.includeAnomalies && !string.IsNullOrEmpty(seaTurtle.anomalies))
            {
                sb.Append($"Anomalies: {seaTurtle.anomalies}; ");
            }

            return sb.ToString();
        }

        private static void FillSectionOneRow(AcroFields acroFields, string fieldNumber, HoldingFacilityReportItem item, MarineTurtleHoldingFacilityQuarterlyReportOptionsDto reportOptions)
        {
            // always set the status/tag number field as it may be a partial line
            acroFields.SetField($"txtTagNumber{fieldNumber}", item.reportTagNumberFieldData);

            // if NOT a partial line...
            if (!string.IsNullOrEmpty(item.seaTurtleId))
            {
                // *************************************************************************************************
                // -- Tag Numbers, Acquired From, and Anomalies share the TAG NUMBER field (part of the STATUS field)
                // -- Relinquished To and Stranding ID Number share the RELINQUISHED TO field (part of the DATE RELEASED/.../... field)
                // *************************************************************************************************

                var sidNumberAndSeaTurtleName = item.sidNumber;
                if (reportOptions.includeTurtleName && !string.IsNullOrEmpty(item.seaTurtleName))
                {
                    sidNumberAndSeaTurtleName += $" - {item.seaTurtleName}";
                }
                acroFields.SetField($"txtSID{fieldNumber}", sidNumberAndSeaTurtleName);

                acroFields.SetField($"cboSpecies{fieldNumber}", item.species);

                acroFields.SetField($"txtDateAcquired{fieldNumber}", item.dateAcquired);

                acroFields.SetField($"cboSize{fieldNumber}", item.turtleSize);
                acroFields.SetField($"cboStatus{fieldNumber}", item.status);


                // ----------------------------------------------------------------
                // -- if the DATE RELINQUISHED is later than the report date, 
                // -- then do NOT display the relinquished information
                // ----------------------------------------------------------------
                var showRelinquishedInfo = item.dateRelinquished.CompareTo(reportOptions.dateThru) <= 0;

                acroFields.SetField($"txtDateRelinquished{fieldNumber}", showRelinquishedInfo ? item.dateRelinquished : string.Empty);

                // *************************************************************************************************
                // v-- *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO 
                // *************************************************************************************************
                // ----------------------------------------------------------------
                // -- add STRANDING ID NUMBER information here (if any)
                // ----------------------------------------------------------------
                var txtRelinquishedTo = string.Empty;

                if (!string.IsNullOrEmpty(item.strandingIdNumber))
                {
                    txtRelinquishedTo += $"Stranding ID #: {item.strandingIdNumber};";
                }

                if (showRelinquishedInfo && !string.IsNullOrEmpty(item.relinquishedTo))
                {
                    txtRelinquishedTo += $"Relinq. To: {item.relinquishedTo}";
                }

                acroFields.SetField($"txtRelinquishedTo{fieldNumber}", txtRelinquishedTo);
                // *************************************************************************************************
                // ^-- *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO *** RELINQUISHED TO 
                // *************************************************************************************************
            }
        }

    }

    public class HoldingFacilityReportItem
    {
        public string seaTurtleId { get; set; }
        public string seaTurtleName { get; set; }
        public string sidNumber { get; set; }
        public string strandingIdNumber { get; set; }
        public string species { get; set; }
        public string dateAcquired { get; set; }
        public string acquiredFrom { get; set; }
        public string acquiredCounty { get; set; }
        public string turtleSize { get; set; }
        public string status { get; set; }
        public string dateRelinquished { get; set; }
        public string relinquishedTo { get; set; }
        public string reportTagNumberFieldData { get; set; }
    }
}
